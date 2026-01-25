import bencode from "bencode";
import express from "express";
import { DOMParser, XMLSerializer } from "@xmldom/xmldom";
import xpath from "xpath";
import {
  args,
  CONST,
  logger,
  Movie,
  RadarrApi,
  Series,
  SonarrApi,
  Utils,
} from "@cortexerr/core";
// import { Series, SonarrApi } from "";

const template = `
<?xml version="1.0" encoding="UTF-8"?>
<rss version="2.0"
     xmlns:atom="http://www.w3.org/2005/Atom"
     xmlns:torznab="http://torznab.com/schemas/2015/feed"
     xmlns:newznab="http://www.newznab.com/DTD/2010/feeds/attributes/">
  <channel>
    <title>Torznab</title>
    <atom:link href="http://192.168.1.113:3939" rel="self" type="application/rss+xml" />
  </channel>
</rss>`;

const fake_test = `
<?xml version="1.0" encoding="UTF-8"?>
<caps>
  <server version="2.0"
          title="FakeTorznab"
          strapline="Torznab test stub"
          email="admin@example.com"
          url="http://192.168.1.113:3939/"
          image="http://localhost:9117/logo.png" />
  <limits max="100" default="50" />
  <registration available="no" open="no" />
  <searching>
    <search available="no" supportedParams="q" />
    <tv-search available="yes" supportedParams="tvdbid,season,ep" />
    <movie-search available="yes" supportedParams="tmdbid" />
    <audio-search available="no" supportedParams="q" />
    <book-search available="no" supportedParams="q" />
  </searching>
  <categories>
    <category id="5000" name="TV">
      <subcat id="5030" name="SD" />
      <subcat id="5040" name="HD" />
      <subcat id="5070" name="Anime" />
    </category>
    <category id="2000" name="Movies">
      <subcat id="2040" name="HD" />
    </category>
  </categories>
</caps>
`;

const fake_search = `
<?xml version="1.0" encoding="UTF-8"?>
<rss version="2.0"
     xmlns:torznab="http://torznab.com/schemas/2015/feed"
     xmlns:newznab="http://www.newznab.com/DTD/2010/feeds/attributes/">
  <channel>
    <title>FakeTorznab</title>
    <description>Torznab API results</description>
    <link>http://192.168.1.113:3939/</link>
    <newznab:response offset="0" total="2" />
    <item>
      <title>Some.Show.S01E01.720p.WEB-DL.x264</title>
      <guid isPermaLink="false">fake-torznab-guid-1</guid>
      <pubDate>Wed, 07 Jan 2026 21:20:00 -0500</pubDate>
      <category>TV > HD</category>
      <description>Fake result for testing.</description>
      <enclosure url="http://localhost:9117/download/1.torrent"
                 length="123456789"
                 type="application/x-bittorrent" />
      <newznab:attr name="category" value="5040" />
      <newznab:attr name="category" value="5000" />
      <torznab:attr name="seeders" value="25" />
      <torznab:attr name="leechers" value="3" />
      <torznab:attr name="size" value="123456789" />
    </item>
    <item>
      <title>Some.Show.S01E01.HDTV.x264</title>
      <guid isPermaLink="false">fake-torznab-guid-2</guid>
      <pubDate>Wed, 07 Jan 2026 21:19:00 -0500</pubDate>
      <category>TV > SD</category>
      <description>Fake result for testing.</description>
      <enclosure url="http://localhost:9117/download/2.torrent"
                 length="987654321"
                 type="application/x-bittorrent" />
      <newznab:attr name="category" value="5030" />
      <newznab:attr name="category" value="5000" />
      <torznab:attr name="seeders" value="10" />
      <torznab:attr name="leechers" value="1" />
      <torznab:attr name="size" value="987654321" />
    </item>
  </channel>
</rss>
`;

const app = express();

function torznabAttribute(doc: Document, name: string, value: string) {
  const attribute = doc.createElementNS(
    "http://torznab.com/schemas/2015/feed",
    "torznab:attr",
  );
  attribute.setAttribute("name", name);
  attribute.setAttribute("value", value);
  return attribute;
}

function buildItem(
  doc: Document,
  query: string,
  request: Series | Movie,
  release: string,
  season?: number,
  episode?: number,
): Element {
  const item = doc.createElement("item");

  const release_formated = release.length ? `.${release}` : "";
  const release_name = `${query.replaceAll(" ", ".")}${release_formated}`;
  const generated_name = `${release_name}.${request.year}.1080p.WEB-DL.x264`;

  const title = doc.createElement("title");
  title.appendChild(doc.createTextNode(generated_name));
  item.appendChild(title);
  const guid = doc.createElement("guid");
  guid.setAttribute("isPermaLink", "false");

  const hash = Utils.randomHexadecimal(40);

  guid.appendChild(
    doc.createTextNode(`
      magnet:?xt=urn:btih:${hash}&amp;dn=${generated_name}&amp;tr=http%3a%2f%2ftracker.opentrackr.org%3a1337%2fannounce;
    `),
  );
  item.appendChild(guid);

  const pubDate = doc.createElement("pubDate");
  pubDate.appendChild(doc.createTextNode(Utils.randomUTCDate()));
  item.appendChild(pubDate);

  let random_size = Utils.randomSizeInBytes(20);
  if (
    release.includes("series") ||
    release.includes("season") ||
    release.includes(".S")
  ) {
    random_size = Utils.randomSizeInBytes(100, 20);
  }
  const size = doc.createElement("size");
  size.appendChild(doc.createTextNode(String(random_size)));
  item.appendChild(size);

  let public_id = request.tmdbId;
  let type = "radarr";
  if ("tvdbId" in request) {
    public_id = request.tvdbId;
    type = "sonarr";
  }

  const generated_link = `index/download?query=${query}&hash=${hash}&name=${release_name}&rid=${request.id}&id=${public_id}&type=${type}&year=${request.year}&length=${random_size}&release=${release}${season ? `&season=${season}` : ""}${episode ? `&episode=${episode}` : ""}`;
  const link = doc.createElement("link");
  link.appendChild(doc.createTextNode(generated_link));
  item.appendChild(link);

  const enclosure = doc.createElement("enclosure");
  enclosure.setAttribute("url", generated_link);
  enclosure.setAttribute("length", String(random_size));
  enclosure.setAttribute("type", "application/x-bittorrent");
  item.appendChild(enclosure);

  item.appendChild(torznabAttribute(doc, "tag", "freeleech"));
  item.appendChild(torznabAttribute(doc, "seeders", "100"));
  item.appendChild(torznabAttribute(doc, "peers", "10"));
  if ("tvdbId" in request) {
    item.appendChild(torznabAttribute(doc, "category", "5000"));
  } else {
    item.appendChild(torznabAttribute(doc, "category", "2000"));
  }
  item.appendChild(torznabAttribute(doc, "imdbid", String(request.imdbId)));
  return item;
}

function buildFakeSearchBase() {
  const parser = new DOMParser();
  const doc: Document = parser.parseFromString(template, "application/xml");

  const select = xpath.useNamespaces({
    atom: "http://www.w3.org/2005/Atom",
    torznab: "http://torznab.com/schemas/2015/feed",
  });

  const nodes = select("/rss/channel", doc) as Node[];
  const channel = nodes[0] as Element | undefined;

  if (!channel) {
    throw new Error("RSS channel element not found");
  }
  const serializer = new XMLSerializer();

  return { doc, channel, serializer };
}

function buildFakeSearchRadarrXml(movie: Movie) {
  const { doc, channel, serializer } = buildFakeSearchBase();

  logger.info(movie.sortTitle);
  logger.info(movie.title);
  logger.info(movie.originalTitle);

  const query = movie.sortTitle;

  channel.appendChild(buildItem(doc, query, movie, ""));

  return cleanXml(serializer.serializeToString(doc));
}

function buildFakeSearchSonarrXml(
  series: Series,
  season?: number,
  episode?: number,
) {
  const { doc, channel, serializer } = buildFakeSearchBase();

  const query = series.titleSlug.replaceAll("-", " ");

  // default
  channel.appendChild(buildItem(doc, query, series, "Complete.Series"));
  channel.appendChild(buildItem(doc, query, series, "Full.Series"));
  channel.appendChild(buildItem(doc, query, series, "Season-1"));
  channel.appendChild(buildItem(doc, query, series, "S01"));
  channel.appendChild(buildItem(doc, query, series, "S01E01"));

  // dynamic
  if (season) {
    channel.appendChild(
      buildItem(doc, query, series, `Season-${season}`, season),
    );
    // totalEpisodeCount
    channel.appendChild(
      buildItem(
        doc,
        query,
        series,
        `S${String(season).padStart(2, "0")}`,
        season,
      ),
    );
    const series_season = series.seasons.find((i) => i.seasonNumber == season);
    const episode_count = series_season?.statistics.totalEpisodeCount;

    if (episode_count) {
      for (let i = 1; i <= episode_count; i++) {
        channel.appendChild(
          buildItem(
            doc,
            query,
            series,
            `S${String(season).padStart(2, "0")}E${String(i).padStart(2, "0")}`,
            season,
            episode,
          ),
        );
      }
    }
  }
  if (episode) {
    channel.appendChild(
      buildItem(
        doc,
        query,
        series,
        `S01E${String(episode).padStart(2, "0")}`,
        episode,
      ),
    );
    channel.appendChild(
      buildItem(
        doc,
        query,
        series,
        `E${String(episode).padStart(2, "0")}`,
        episode,
      ),
    );
  }
  if (episode && season) {
    channel.appendChild(buildItem(doc, query, series, `Season-${season}`));
    channel.appendChild(
      buildItem(
        doc,
        query,
        series,
        `Season-${season}-${episode}`,
        season,
        episode,
      ),
    );
    channel.appendChild(
      buildItem(
        doc,
        query,
        series,
        `S${String(season).padStart(2, "0")}E${String(episode).padStart(2, "0")}`,
        season,
        episode,
      ),
    );
    for (let i = 1; i <= episode; i++) {
      channel.appendChild(
        buildItem(
          doc,
          query,
          series,
          `S${String(season).padStart(2, "0")}E${String(i).padStart(2, "0")}`,
          season,
          episode,
        ),
      );
    }
  }
  return cleanXml(serializer.serializeToString(doc));
}

function cleanXml(s: string) {
  if (s.charCodeAt(0) === 0xfeff) s = s.slice(1);
  return s.replace(/^\s+/, "");
}

// first hit mitm proxy request modification
app.use(async (req, res, next) => {
  logger.info("---- INDEXER ----");
  logger.info([req.method, req.originalUrl]);
  logger.info(req.headers);

  const url = new URL(req.originalUrl, `http://${req.headers.host}`);
  const params = url.searchParams;
  // console.log(params);

  if (req.originalUrl.startsWith("/index/download")) {
    const fake_torrent = {
      announce: "http://tracker.opentrackr.org:1337/announce",
      info: {
        name: params.get("name"),
        hash: params.get("hash"),
        query: params.get("query"),
        rid: params.get("rid"),
        id: params.get("id"),
        type: params.get("type"),
        year: params.get("year"),
        season: params.get("season"),
        episode: params.get("episode"),
        "piece length": 524288,
        pieces: Buffer.alloc(20 * 8, 0),
        length: params.get("length"),
        release: params.get("release"),
      },
    };

    const torrent_data = bencode.encode(fake_torrent);
    res
      .status(200)
      .set("Content-Type", "application/x-bittorrent")
      .send(torrent_data);
    return;
  }

  // block non valid apikey
  const api = params.get("apikey");
  if (api != CONST.JACKETT_API_KEY) {
    logger.warn(`${url} - Unauthorized`);
    res.status(401).send("Unauthorized");
    return;
  }

  const tvdbid = params.get("tvdbid");
  if (tvdbid) {
    try {
      const season = params.get("season");
      const episode = params.get("ep") ?? params.get("episode");

      const series = await SonarrApi.getSeries(Number(tvdbid));
      if (!series) {
        res.status(500).send("Missing series");
        return;
      }

      const rss = buildFakeSearchSonarrXml(
        series,
        season ? Number(season) : undefined,
        episode ? Number(episode) : undefined,
      );

      res
        .status(200)
        .set("Content-Type", "application/xml; charset=utf-8")
        .send(rss);
      return;
    } catch (err) {
      logger.error(err);
      res.status(500).send(err);
      return;
    }
  }

  const tmdbid = params.get("tmdbid");
  if (tmdbid) {
    try {
      const movie = await RadarrApi.getMovie(Number(tmdbid));
      if (!movie) {
        res.status(500).send("Missing movie");
        return;
      }

      const rss = buildFakeSearchRadarrXml(movie);

      res
        .status(200)
        .set("Content-Type", "application/xml; charset=utf-8")
        .send(rss);
      return;
    } catch (err) {
      logger.error(err);
      res.status(500).send(err);
      return;
    }
  }

  // fake testing
  const type = params.get("t");
  if (type == "caps") {
    try {
      const rss = cleanXml(fake_test);
      res
        .status(200)
        .set("Content-Type", "application/xml; charset=utf-8")
        .send(rss);
      return;
    } catch (err) {
      logger.error(err);
      res.status(500).send(err);
      return;
    }
  }
  if (type == "tvsearch" || type == "movie") {
    try {
      const rss = cleanXml(fake_search);
      res
        .status(200)
        .set("Content-Type", "application/xml; charset=utf-8")
        .send(rss);
      return;
    } catch (err) {
      logger.error(err);
      res.status(500).send(err);
      return;
    }
  }
});

app.listen(3939, args.host.hostname, () => {
  logger.info(`[Indexer_Proxy] listening on ${args.host.hostname}:${3939}`);
});

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
