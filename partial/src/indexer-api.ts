import {
  args,
  CONST,
  Hydra,
  IngestCategory,
  Jackett,
  JSONSearchResultNewznab,
  JSONSearchResultTorznab,
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

// controls the search query for torrents, public_id for usenet, either 1 can be included or both if both then u get both searches.
type Search = { query?: string; public_id?: string };

export type IndexerSearch = {
  searches: Search[];
  category: IngestCategory;
  season?: number | undefined;
  episode?: number | undefined;
};

export type AutoIndexerSearch = {
  search: Search;
  category: IngestCategory;
  season?: number;
  episode?: number;
};

export class Indexer {
  static #validateQuery(searches: Search[]) {
    for (let search of searches) {
      if (!search.query && !search.public_id) {
        return errorResponse(
          "INVALID_INPUT",
          "Expected (searches: Search[]) public_id and query parameters missing at least one must be added!",
        );
      }
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
    return errorResponse("INVALID_INPUT", "Unsupported IngestCategory!");
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
    return errorResponse("INVALID_INPUT", "Unsupported IngestCategory!");
  }

  public static async manualSearch({
    category,
    season,
    episode,
    searches,
  }: IndexerSearch) {
    const validation = this.#validateQuery(searches);
    if (!validation.success) {
      return validation; // error state
    }

    const searched_results = [];
    for (let search of searches) {
      // handle cached searches
      const cached_public_id = cached_indexer_results.get(
        String(search.public_id),
      );
      const cached_query = cached_indexer_results.get(String(search.query));

      if (cached_public_id && cached_query) {
        searched_results.push(successResponse(cached_public_id));
        searched_results.push(successResponse(cached_query));
        continue;
      } else if (cached_public_id) {
        searched_results.push(successResponse(cached_public_id));
      } else if (cached_query) {
        searched_results.push(successResponse(cached_query));
      }

      // search for uncached
      let search_result_usenet = undefined;
      let search_result_torrent = undefined;

      if (search.public_id && !cached_public_id) {
        search_result_usenet = await this.#usenetSearch(
          search.public_id,
          category,
          season,
          episode,
        );
      }
      if (search.query && !cached_query) {
        search_result_torrent = await this.#torrentSearch(
          search.query,
          category,
          season,
          episode,
        );
      }

      // update cache
      if (search_result_usenet?.success) {
        searched_results.push(search_result_usenet);
        cached_indexer_results.set(
          String(search.public_id),
          search_result_usenet.data,
        );
      }
      if (search_result_torrent?.success) {
        searched_results.push(search_result_torrent);
        cached_indexer_results.set(
          String(search.query),
          search_result_torrent.data,
        );
      }
    }

    if (searched_results.length) {
      return successResponse(searched_results);
    } else {
      return errorResponse(
        "UNEXPECTED_ERROR",
        "Searches failed to provided any responses.",
      );
    }
  }

  public static search({
    search,
    category,
    season,
    episode,
  }: AutoIndexerSearch) {
    if (!search.query?.length && search.public_id?.length) {
      return errorResponse(
        "INVALID_INPUT",
        "Expected (search: Search) at least one parameter is required!",
      );
    }

    const searches: Search[] = [];

    // defaults need to be carfully decided likely will change only for torrents
    if (search.query && args.release_groups) {
      searches.push({ query: search.query });

      for (let group of args.release_groups) {
        searches.push({ query: `${search.query} ${group}` });
      }
    }
    if (search.public_id) {
      searches.push({ public_id: search.public_id });
    }

    return this.manualSearch({ searches, category, season, episode });
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
