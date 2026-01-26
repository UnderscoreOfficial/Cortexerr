import {
  args,
  ingest,
  IngestRequest,
  IngestRequestBase,
  IngestState,
  logger,
} from "@cortexerr/core";

import bencode from "bencode";
import express from "express";
import multer from "multer";

type BuildQueueResponse = {
  hash: string;
  name?: string;
  size?: number;
  progress: number;
  dlspeed: number;
  state: IngestState;
  category: string;
  save_path: string;
  content_path?: string;
  amount_left?: number;
  completion_on?: number;
  eta: number;
};

const upload = multer();

const app = express();
app.use(express.urlencoded({ extended: true })); // Parse POST form data

const categories: Record<string, {}> = {
  tv_sonarr: { savePath: args.sonarr_download_path },
  movie_radarr: { savePath: args.radarr_download_path },
};

function toStringValue(value: unknown): string {
  if (value instanceof Uint8Array) {
    return Buffer.from(value).toString("utf8");
  }
  if (typeof value === "object" && value !== null) {
    return JSON.stringify(value, null, 2);
  }
  return String(value);
}

// arrs request to downloader to add
app.post(
  "/api/v2/torrents/add",
  upload.single("torrents"),
  async (req, res) => {
    if (!req.file) {
      console.log("No file uploaded.");
      console.log(req.body);
      res.status(200).send("");
      return;
    }

    // decoded params from torrent blob
    try {
      const decoded = bencode.decode(req.file.buffer);
      const params = [
        "hash",
        "name",
        "query",
        "rid",
        "id",
        "type",
        "year",
        "season",
        "episode",
        "release",
        "length",
      ];
      const fields = new Map();

      for (let param of params) {
        fields.set(param, toStringValue(decoded.info[param]));
      }
      // fix categories once I add radarr support
      const hash = fields.get("hash");
      if (!ingest.has(hash)) {
        const request_base: IngestRequestBase = {
          category: fields.get("type"),
          search_query: String(fields.get("query")),
          rid: String(fields.get("rid")),
          year: Number(fields.get("year")),
          length: Number(fields.get("length")),
        };

        let request: IngestRequest | undefined;

        if (fields.get("type") == "sonarr") {
          request = {
            ...request_base,
            category: "tv_sonarr",
            tvdb_id: Number(fields.get("id")),
            release: String(fields.get("release")),
          };
          if (fields.get("season")) {
            request.season = fields.get("season");
          }
          if (fields.get("episode")) {
            request.episode = fields.get("episode");
          }
        } else if (fields.get("type") == "sonarr") {
          request = {
            ...request_base,
            category: "movie_radarr",
            tmdb_id: String(fields.get("id")),
          };
        }

        if (request) {
          ingest.set(hash, {
            hash,
            status: {
              progress: 0,
              state: "downloading",
              save_path:
                String(fields.get("type")) == "sonarr"
                  ? args.sonarr_download_path
                  : args.radarr_download_path,
              completed: false,
            },
            request,
          });
          logger.info("-- TORRENT DOWNLOADING --");
        }
        // error state figure out error handeling at core
      }
    } catch (err) {
      console.error("Failed to decode torrent:", err);
    }

    res.status(200).send("");
  },
);

app.use(async (req, res) => {
  logger.info("---- DOWNLOADER ----");
  logger.info([req.method, req.originalUrl]);
  logger.info(req.headers);

  const url = new URL(req.originalUrl, `http://${req.headers.host}`);
  const params = url.searchParams;

  if (req.originalUrl == "/api/v2/app/webapiVersion") {
    res
      .status(200)
      .set("Content-Type", "text/plain; charset=utf-8")
      .send("2.8.3");
    return;
  }

  if (req.originalUrl == "/api/v2/app/preferences") {
    res.status(200).json({
      // save_path: args.download_path,
      temp_path_enabled: false,
      temp_path: "",
      scan_dirs: {},
      auto_tmm_enabled: false,
      torrent_content_layout: "Original",
      start_paused_enabled: false,
      auto_delete_mode: 0,
      preallocate_all: false,
      incomplete_files_ext: false,
      web_ui_username: "admin",
      save_resume_data_interval: 15,
    });
    return;
  }

  if (req.originalUrl == "/api/v2/torrents/categories") {
    res.status(200).json(categories);
    return;
  }

  if (
    req.originalUrl == "/api/v2/torrents/createCategory" &&
    req.method == "POST"
  ) {
    console.log(req.body);
    const name = req.body.category;
    // const savePath = req.body.savePath || `/downloads/${name || "default"}/`;
    if (name) {
      categories[name] = {};
      logger.info(`Added category ${name}`);
    }
    res.status(200).send("");
    return;
  }

  if (req.originalUrl.startsWith("/api/v2/torrents/info")) {
    const response_queue = [];
    const category = params.get("category");
    if (!(category == "tv_sonarr" || category == "movie_radarr")) return;

    for (const torrent of ingest.values()) {
      const status = torrent.status;
      const request = torrent.request;

      const build_queue_response: BuildQueueResponse = {
        hash: torrent.hash,
        progress: status.progress ?? 0,
        dlspeed: status.dlspeed ?? 0,
        state: status.state,
        category: request.category,
        save_path: status.save_path,
        completion_on: status.completed ? Math.floor(Date.now() / 1000) : 0,
        eta: status.eta ?? 0,
      };

      if (status.size) {
        build_queue_response.size = status.size;
      }
      if (status.content_path) {
        build_queue_response.content_path = status.content_path;
      }
      if (status.amount_left) {
        build_queue_response.amount_left = status.completed
          ? 0
          : status.size
            ? status.size * (1 - status.progress)
            : request.length;
      }

      // only fake values temporarily used until real downloads are happening, data is from proxy indexer
      if (request.category == "tv_sonarr") {
        build_queue_response.name = `${request.search_query.replaceAll(" ", ".")}.${request.release}`;
      } else {
        build_queue_response.name = request.search_query.replaceAll(" ", ".");
      }

      response_queue.push(build_queue_response);
    }

    res.status(200).json(response_queue);
    return;
  }

  // (does not delete from downloader actual downloader deletion is done separately to ensure any other requests get correct files)
  if (req.originalUrl == "/api/v2/torrents/delete" && req.method == "POST") {
    const { hashes } = req.body;

    if (!hashes) {
      res.status(400).send("Missing hashes");
      return;
    }

    const list = hashes.split("|");
    for (const hash of list) {
      if (ingest.has(hash)) {
        ingest.delete(hash);
        logger.warn(`Deleted torrent ${hash}`);
      }
    }
    res.status(200);
    return;
  }
  res.status(404).send("Not Found");
});

app.listen(8282, args.host.hostname, () => {
  logger.info(`[Downloader_Proxy] listening on ${args.host.hostname}:${8282}`);
});

// block non valid apikey
// const api = params.get("apikey");
// if (api != CONST.JACKETT_API_KEY) {
//   logger.warn(`${url} - Unauthorized`);
//   res.status(401).send("Unauthorized");
//   return;
// }
//

// const delay_hours = (Number(args["cache-time"]) ?? CACHE_TIME) * 60 * 1000 // 1 hour

// function purgeStaleCache() {
//   const now = Date.now();
//   logger.warn("Purging Stale Cache!");
//   for (let [key, entry] of cache.entries()) {
//     if (!entry || now - entry.timestamp > delay_hours) {
//       cache.delete(key);
//     }
//   }
// }

// setInterval(purgeStaleCache, delay_hours);
