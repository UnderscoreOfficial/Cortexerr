// General structure should be communicate used user defined indexer choice if it has a method to get its indexers
// ->
// decide search general? specific search does cost factor in per api hit or not? are you including specific ep/season or scene group info
// ->
// preform search upon only enabled indexers in parallel, aggragate results.
import { DOMParser, XMLSerializer } from "@xmldom/xmldom";
import { XMLParser } from "fast-xml-parser";
import { args, CONST, logger } from "./args.js";
import xpath from "xpath";

const parser = new XMLParser({
  ignoreAttributes: false,
  attributeNamePrefix: "",
});

// avaliable indexer types
type JackettIndexer = {
  title: string;
  description: string;
  link: string;
  language: string;
  type: string;
  caps: {
    server: {
      title: string;
    };
    limits: {
      default: string;
      max: string;
    };
    searching: {
      search: {
        available: string;
        supportedParams: string;
      };
      "tv-search": {
        available: string;
        supportedParams: string;
      };
      "movie-search": {
        available: string;
        supportedParams: string;
      };
    };
    categories: {
      category: Array<{
        id: string;
        name: string;
        subcat?: Array<{
          id: string;
          name: string;
        }>;
      }>;
    };
  };
  id: string;
  configured: string | boolean;
};
type HydraIndexer = {
  indexer: string;
  state: string;
  level: number;
  disabledUntil: string | null;
  lastError: string | null;
  apiResetTime: string | null;
  downloadResetTime: string | null;
  apiHits: number;
  apiHitLimit: number;
  downloadHits: number;
  downloadHitLimit: number;
  vipExpirationDate?: string;
};

// merged search types
type JSONSearchResultBase = {
  title: string;
  comments: string;
  pubDate: string;
  size: number;
  description: string;
  link: string;
  enclosure: {
    url: string;
    length: string;
    type: string;
  };
};

export type JSONSearchResultTorznab = JSONSearchResultBase & {
  jackettindexer: {
    "#text": string;
    id: string;
  };
  guid: string;
  type: string;
  files: number;
  link: string;
  category: number[];
  "torznab:attr"?: Array<
    | { name: "seeders"; value: string }
    | { name: "peers"; value: string }
    | { name: "infohash"; value: string }
    | { name: "magneturl"; value: string }
  >;
};
export type JSONSearchResultNewznab = JSONSearchResultBase & {
  guid: {
    "#text": string;
    isPermaLink: string;
  };
  "newznab:attr"?: Array<
    | { name: "category"; value: string }
    | { name: "season"; value: string }
    | { name: "episode"; value: string }
    | { name: "files"; value: string }
    | { name: "grabs"; value: string }
    | { name: "group"; value: string }
    | { name: "sha1"; value: string }
    | { name: "hydraIndexerScore"; value: string }
    | { name: "hydraIndexerHost"; value: string }
    | { name: "hydraIndexerName"; value: string }
  >;
};

// api args
type ApiArgs = {
  type: string;
  api_base: string;
  query?: string | undefined;
  id?: string | undefined;
  categories?: string[] | undefined;
  season?: number | undefined;
  episode?: number | undefined;
};

// search methods types
type Search = {
  api_base?: string; // change at your own risk can break stuff if not formated correctly
  categories?: string[];
};

type TvSearchBase = Search & {
  season?: number | undefined;
  episode?: number | undefined;
};
export type TvSearchTorrents = TvSearchBase & {
  query: string;
  tvdb_id?: string;
};
export type TvSearchUsenet = TvSearchBase & {
  tvdb_id: string;
  query?: string;
};

export type MovieSearchTorrents = Search & {
  query: string;
  tmdb_id?: string;
};
export type MovieSearchUsenet = Search & {
  tmdb_id: string;
  query?: string;
};

class NabApiBase {
  protected static async requestIndexers(indexer_urls: string[]) {
    const xml_docs = await Promise.all(
      indexer_urls.map(async (url) => {
        const res = await fetch(`${url}`);
        if (!res.ok) return null;

        const xml = await res.text();

        // parsing raw data for errors before cleanly grabing the unmodified xml data
        const json_data = parser.parse(xml);
        if (json_data.error) {
          if (json_data.error.description) {
            logger.error(json_data.error.description);
            return null;
          }
        }
        // must be formated with dom parser fast-xml-parser caused formating issues
        const doc = new DOMParser().parseFromString(xml, "application/xml");
        if (doc.getElementsByTagName("parsererror").length) return null;
        return doc;
      }),
    );
    if (!xml_docs) throw Error("xml_docs is undefined or missing?");
    return xml_docs;
  }
  protected static mergeFormatResults<T>(xml_docs: (null | Document)[]) {
    const valid_docs = xml_docs.filter((d) => d !== null);
    if (valid_docs.length == 0) throw new Error("No indexers.");

    const xml_items = valid_docs.flatMap(
      (doc) => xpath.select("//channel/item", doc) as Node[],
    );

    // use rss xml response as base template to avoid incompatibilities
    const rss_template = valid_docs[0];
    if (!rss_template) throw new Error("Template Missing.");

    const channel = xpath.select1("//channel", rss_template) as Node;

    for (const item of xpath.select("./item", channel) as Node[]) {
      channel.removeChild(item);
    }
    for (const item of xml_items) {
      channel.appendChild(rss_template.importNode(item, true));
    }

    const xml_rss_feed = new XMLSerializer().serializeToString(rss_template);
    const json_result = parser.parse(xml_rss_feed);

    return {
      xml: xml_rss_feed,
      json: Array.isArray(json_result?.rss?.channel?.item)
        ? (json_result.rss.channel.item as T[])
        : json_result?.rss?.channel?.item
          ? ([json_result.rss.channel.item] as T[])
          : ([] as T[]),
      length: xml_items.length,
    };
  }

  protected static buildApi({
    type,
    api_base,
    query,
    id,
    categories,
    season,
    episode,
  }: ApiArgs) {
    const api = new URLSearchParams(`${api_base}&t=${type}`);
    // let api = `${api_base}&t=${type}`;
    if (type == "tvsearch") {
      api.append("cat", "5000");
      if (id) {
        api.append("tvdbid", id);
      }
      if (season) {
        api.append("season", String(season));
      }
      if (episode) {
        api.append("ep", String(episode));
      }
    } else if (type == "movie") {
      api.append("cat", "2000");
      if (id) {
        api.append("tmdbid", id);
      }
    }
    if (categories) {
      for (let category of categories) {
        api.append("cat", category);
      }
    }
    if (query) {
      api.append("q", query);
    }
    return api;
  }
}
export class Jackett extends NabApiBase {
  // by default get all enabled indexers any further filtering should happen after search
  static async #getIndexers() {
    const url = `${args.jackett.origin}/api/v2.0/indexers/all/results/torznab/api?apikey=${CONST.JACKETT_API_KEY}&t=indexers`;

    const response = await fetch(url);
    if (!response.ok) {
      throw new Error(`Error fetching Jackett data: ${response.statusText}`);
    }

    const xml_text = await response.text();
    const parsed = parser.parse(xml_text);

    // Extract indexers array from the parsed structure
    const indexers_data = parsed.indexers?.indexer || [];

    // Ensure it's an array (single item becomes object)
    const indexers_array = Array.isArray(indexers_data)
      ? indexers_data
      : [indexers_data];

    const active_indexers = indexers_array.filter(
      (indexer) => indexer.configured == "true" || indexer.configured == true,
    );

    if (!active_indexers.length)
      throw Error(`Failed to fetch any valid indexers from provider. ${url}`);
    return active_indexers as JackettIndexer[];
  }

  static #getIndexerUrls(indexers: JackettIndexer[], api: URLSearchParams) {
    const valid_indexers: string[] = [];
    for (let indexer of indexers) {
      // remove unsupported search params from indexers & disabled non supported types
      const type = api.get("t");
      if (!type) throw Error("No valid type in api");

      let supported_params: string[];
      let available: boolean = false;

      if (type == "tvsearch") {
        const tv_search = indexer.caps.searching["tv-search"];
        supported_params = tv_search.supportedParams.split(",");
        available = tv_search.available == "yes" ? true : false;
      } else if (type == "movie") {
        const movie_search = indexer.caps.searching["movie-search"];
        supported_params = movie_search.supportedParams.split(",");
        available = movie_search.available == "yes" ? true : false;
      }

      if (!available) {
        continue;
      }

      const params = ["ep", "season", "tvdbid", "tmdbid", "q"];
      const invalid_params = params.filter(
        (param) => !supported_params.includes(param),
      );
      for (let param of invalid_params) {
        api.delete(param);
      }

      valid_indexers.push(
        `${args.jackett.origin}/api/v2.0/indexers/${indexer.id}/results/torznab/api?${api}`,
      );
    }
    return valid_indexers;
  }

  static async #search(api: URLSearchParams) {
    const indexers = await this.#getIndexers();
    const indexer_urls = this.#getIndexerUrls(indexers, api);
    const requested_indexers = await this.requestIndexers(indexer_urls);
    const formatted_indexers =
      this.mergeFormatResults<JSONSearchResultTorznab>(requested_indexers);

    return formatted_indexers;
  }

  // search type and api shape is defined here. extra api params can be done via the api_base
  // changing api_base will break stuff if you don't know what your doing,
  public static async tvSearch({
    api_base,
    categories,
    season,
    episode,
    query,
    tvdb_id,
  }: TvSearchTorrents) {
    const api = this.buildApi({
      type: "tvsearch",
      api_base:
        api_base ??
        `apikey=${CONST.JACKETT_API_KEY}&extended=1&offset=0&limit=100`,
      query,
      id: tvdb_id,
      categories,
      season,
      episode,
    });
    return this.#search(api);
  }

  public static movieSearch({
    api_base,
    categories,
    query,
    tmdb_id,
  }: MovieSearchTorrents) {
    const api = this.buildApi({
      type: "movie",
      api_base:
        api_base ??
        `apikey=${CONST.JACKETT_API_KEY}&extended=1&offset=0&limit=100`,
      query,
      id: tmdb_id,
      categories,
    });
    return this.#search(api);
  }
}

export class Hydra extends NabApiBase {
  // by default get all enabled indexers any further filtering should happen after search
  static async #getIndexers() {
    const url = `${args.hydra.origin}/api/stats/indexers`;

    const response = await fetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        apikey: CONST.HYDRA_API_KEY,
      }),
    });

    if (!response.ok) {
      throw new Error(`Error fetching Hydra data: ${response.statusText}`);
    }
    const indexers = (await response.json()) as HydraIndexer[];
    const active_indexers = indexers.filter(
      (indexer) => indexer.state == "ENABLED",
    );
    return active_indexers;
  }

  static #getIndexerUrls(indexers: HydraIndexer[], api: URLSearchParams) {
    return indexers.map(
      (indexer) =>
        `${args.hydra.origin}/api?${api}&indexers=${indexer.indexer}`,
    );
  }

  static async #search(api: URLSearchParams) {
    const indexers = await this.#getIndexers();
    const indexer_urls = this.#getIndexerUrls(indexers, api);
    const requested_indexers = await this.requestIndexers(indexer_urls);
    const formatted_indexers =
      this.mergeFormatResults<JSONSearchResultNewznab>(requested_indexers);
    //
    return formatted_indexers;
  }

  public static async tvSearch({
    api_base,
    categories,
    season,
    episode,
    query,
    tvdb_id,
  }: TvSearchUsenet) {
    const api = this.buildApi({
      type: "tvsearch",
      api_base: api_base ?? `apikey=${CONST.HYDRA_API_KEY}&offset=0&limit=100`,
      query,
      id: tvdb_id,
      categories,
      season,
      episode,
    });
    return this.#search(api);
  }

  public static async movieSearch({
    api_base,
    categories,
    query,
    tmdb_id,
  }: MovieSearchUsenet) {
    const api = this.buildApi({
      type: "movie",
      api_base: api_base ?? `apikey=${CONST.HYDRA_API_KEY}&offset=0&limit=100`,
      query,
      id: tmdb_id,
      categories,
    });
    return this.#search(api);
  }
}
