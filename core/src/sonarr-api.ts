// really only neede for the indexer proxy but ill extend it out to all the apis just incase you wan't more data to track
// as well I might end up adding more data form these to the base IngestRequest table anyways to worth extending everywhere

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
  imdbId: string | null;
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

export class SonarrApi {
  static #HOST = args.sonarr.origin;

  static async #request(method: string, endpoint: string, body?: {} | []) {
    if (method == "POST") {
      const req = await request(`${this.#HOST}${endpoint}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "X-Api-Key": CONST.SONARR_API_KEY,
        },
        body: JSON.stringify(body),
      });
      return await req.body.json();
    } else {
      const req = await request(`${this.#HOST}${endpoint}`, {
        method: "GET",
        headers: {
          "X-Api-Key": CONST.SONARR_API_KEY,
        },
      });
      return await req.body.json();
    }
  }
  public static async getQueue() {
    const data: unknown = await this.#request("GET", `/api/v3/queue`);
    logger.info(data);
    // const typed_data = data as Series[];
    // if (typed_data[0]) {
    //   return typed_data[0];
    // }
    // return undefined;
  }
  public static async getSeries(tvdb_id: number) {
    const data: unknown = await this.#request(
      "GET",
      `/api/v3/series?tvdbId=${tvdb_id}`,
    );
    // console.log(JSON.stringify(data));
    const typed_data = data as Series[];
    if (typed_data[0]) {
      return typed_data[0];
    }
    return undefined;
  }
}

// example call - what download clients api call after completed with the file name or path
// SonarrApi.import(
//   "FXs.A.Christmas.Carol.2019.2160p.DSNP.WEB-DL.DDP5.1.HDR.H.265-Kitsune.mkv",
// );

// const api =
//   "/api/v3/manualimport?folder=/shared/www.Torrenting.com - South Park S09E06 The Death of Eric Cartman 1080p HMAX WEB-DL DD5 1 H 264-CtrlHD";
// const api = "/api/v3/command";
// const api = "/api/v3/manualimport";
// const api = "/api/v3/series?tvdbId=75897";
