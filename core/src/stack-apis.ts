import { request } from "undici";
import { args, CONST, logger } from "./args.js";

// types

export type Series = {
  title: string;
  sortTitle: string;
  status: string;
  ended: boolean;
  overview: string;
  year: number;
  path: string;
  qualityProfileId: number;
  seasonFolder: boolean;
  monitored: boolean;
  monitorNewItems: string;
  tvdbId: number;
  tvRageId: number;
  tvMazeId: number;
  tmdbId: number;
  seriesType: string;
  cleanTitle: string;
  imdbId: string;
  titleSlug: string;
  rootFolderPath: string;
  added: string;
  languageProfileId: number;
  id: number;
  seasons: {
    seasonNumber: number;
    monitored: boolean;
    statistics: {
      previousAiring?: string;
      episodeFileCount: number;
      episodeCount: number;
      totalEpisodeCount: number;
      sizeOnDisk: number;
      releaseGroups: unknown[];
      percentOfEpisodes: number;
    };
  }[];
};

export type Movie = {
  title: string;
  originalTitle: string;
  originalLanguage: {
    id: number;
    name: string;
  };
  secondaryYearSourceId: number;
  sortTitle: string;
  sizeOnDisk: number;
  status: string;
  overview: string;
  inCinemas: string;
  physicalRelease: string;
  digitalRelease: string;
  releaseDate: string;
  website: string;
  year: number;
  studio: string;
  path: string;
  qualityProfileId: number;
  hasFile: boolean;
  movieFileId: number;
  monitored: boolean;
  minimumAvailability: string;
  isAvailable: boolean;
  folderName: string;
  runtime: number;
  cleanTitle: string;
  imdbId: string;
  tmdbId: number;
  titleSlug: string;
  rootFolderPath: string;
  added: string;
  id: number;
};

class RequestApiBase {
  protected static async request(
    method: string,
    endpoint: string,
    url: URL,
    api_key: string,
    body?: {} | [],
  ) {
    if (method == "POST") {
      const req = await request(`${url.origin}${endpoint}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "X-Api-Key": api_key,
        },
        body: JSON.stringify(body),
      });
      return await req.body.json();
    } else {
      const req = await request(`${url.origin}${endpoint}`, {
        method: "GET",
        headers: {
          "X-Api-Key": api_key,
        },
      });
      return await req.body.json();
    }
  }
}

export class SonarrApi extends RequestApiBase {
  public static async getQueue() {
    const data: unknown = await this.request(
      "GET",
      `/api/v3/queue`,
      args.sonarr,
      String(CONST.SONARR_API_KEY),
    );
    logger.info(data);
  }
  public static async getSeries(tvdb_id: number) {
    const data: unknown = await this.request(
      "GET",
      `/api/v3/series?tvdbId=${tvdb_id}`,
      args.sonarr,
      String(CONST.SONARR_API_KEY),
    );
    // console.log(JSON.stringify(data));
    const typed_data = data as Series[];
    if (typed_data[0]) {
      return typed_data[0];
    }
    return undefined;
  }
}

export class RadarrApi extends RequestApiBase {
  public static async getQueue() {
    const data: unknown = await this.request(
      "GET",
      `/api/v3/queue`,
      args.radarr,
      String(CONST.RADARR_API_KEY),
    );
    logger.info(data);
  }
  public static async getMovie(tmdb_id: number) {
    const data: unknown = await this.request(
      "GET",
      `/api/v3/movie?tmdbId=${tmdb_id}`,
      args.radarr,
      String(CONST.RADARR_API_KEY),
    );
    const typed_data = data as Movie[];
    if (typed_data[0]) {
      return typed_data[0];
    }
    return undefined;
  }
}
