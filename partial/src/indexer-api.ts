import {
  Hydra,
  ingest,
  IngestCategory,
  Jackett,
  JSONSearchResultNewznab,
  JSONSearchResultTorznab,
  Sabnzbd,
  TvSearchTorrents,
  TvSearchUsenet,
} from "@cortexerr/core";
import {
  errorResponse,
  handleErrorAsync,
  successResponse,
} from "./error-handling.js";

type CachedIndexerResults = {
  xml: string;
  json: JSONSearchResultTorznab[] | JSONSearchResultNewznab[];
  length: number;
};

// should the map key should tie to the search id or query
const cached_indexer_results = new Map<string, CachedIndexerResults>();

type Search =
  | { query: string; public_id?: undefined }
  | { public_id: string | number; query?: undefined };

export type SearchSource = "usenet" | "torrents"; // defaults to all enabled added indexers

export type IndexerSearch = {
  searches: Search[];
  category: IngestCategory;
  source?: SearchSource;
  season?: number;
  episode?: number;
  // not sure if I should include these or only pull from config
};

export class Indexer {
  static #validateQuery(searches: Search[]) {
    for (let search of searches) {
      if (search.query) {
        if (!search.query.length) {
          return errorResponse(
            "INVALID_INPUT",
            "Expected (searches: Search[]) query parameter either missing or invalid!",
          );
        }
      }
      if (search.public_id) {
        if (!String(search.public_id).length) {
          return errorResponse(
            "INVALID_INPUT",
            "Expected (searches: Search[]) public_id parameter either missing or invalid!",
          );
        }
      }
      if (!search.query && !search.public_id) {
        return errorResponse(
          "INVALID_INPUT",
          "Expected (searches: Search[]) public_id and query parameters missing one must be added!",
        );
      }
      if (search.query && search.public_id) {
        return errorResponse(
          "INVALID_INPUT",
          "Expected (searches: Search[]) both public_id and query parameters exist only one can be added!",
        );
      }
    }
    return successResponse();
  }

  static #usenetSearch(
    id: string | number | undefined,
    category: IngestCategory,
    season: number | undefined,
    episode: number | undefined,
  ) {
    if (!id)
      return errorResponse(
        "INVALID_INPUT",
        "Expected (searched: Search[]) public_id parameter not found!",
      );
    if (category == "tv_sonarr") {
      return handleErrorAsync(async () => {
        return await Hydra.tvSearch({
          tvdb_id: String(id),
          season,
          episode,
        });
      });
    } else if (category == "movie_radarr") {
      return handleErrorAsync(async () => {
        return await Hydra.movieSearch({
          tmdb_id: String(id),
        });
      });
    }
  }

  static #torrentSearch(
    query: string | number | undefined,
    category: IngestCategory,
    season: number | undefined,
    episode: number | undefined,
  ) {
    if (!query)
      return errorResponse(
        "INVALID_INPUT",
        "Expected (searched: Search[]) query parameter not found!",
      );
    if (category == "tv_sonarr") {
      return handleErrorAsync(async () => {
        return await Jackett.tvSearch({
          query: String(query),
          season,
          episode,
        });
      });
    } else if (category == "movie_radarr") {
      return handleErrorAsync(async () => {
        return await Jackett.movieSearch({
          query: String(query),
        });
      });
    }
  }

  public static async search({
    // grab these from the config optinal params
    // api_base,
    // categories,
    category,
    source,
    season,
    episode,
    searches,
  }: IndexerSearch) {
    const validation = this.#validateQuery(searches);
    if (!validation.success) {
      return validation; // error state
    }

    for (let search of searches) {
      if (
        cached_indexer_results.get(String(search.public_id)) ||
        cached_indexer_results.get(String(search.query))
      )
        return errorResponse(
          "ALREADY_EXISTS",
          "Requested search item already exists in cached_indexer_results!",
        );
      if (source == "usenet") {
        let search_result = await this.#usenetSearch(
          search.public_id,
          category,
          season,
          episode,
        );
        if (search_result?.success) {
          cached_indexer_results.set(
            String(search.public_id),
            search_result.data,
          );
        } else {
          return search_result;
        }
      } else if (source == "torrents") {
        let search_result = await this.#torrentSearch(
          search.query,
          category,
          season,
          episode,
        );
        if (search_result?.success) {
          cached_indexer_results.set(String(search.query), search_result.data);
        } else {
          return search_result;
        }
      } else {
        let search_result = await this.#usenetSearch(
          search.public_id,
          category,
          season,
          episode,
        );
        if (search_result?.success) {
          cached_indexer_results.set(
            String(search.public_id),
            search_result.data,
          );
        } else {
          return search_result;
        }
      }
    }
  }
}

// constructor(url: string) {
//   // validate url
//   const trimed_url = url.trim();
//   if (
//     trimed_url.startsWith("magnet:?") ||
//     trimed_url.includes("jackett_apikey=")
//   ) {
//     // valid torrent
//   } else {
//     const url = handleError(() => new URL(trimed_url));
//     if (url) {
//       // valid usenet
//     }
//   }
//   this.#url = url;
// }
