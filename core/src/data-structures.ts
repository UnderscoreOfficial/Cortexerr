export type IngestCategory = "tv_sonarr" | "movie_radarr";

export type IngestRequestBase = {
  rid: string;
  search_query: string;
  year: number;
  length: number;
  category: IngestCategory;
};

export type IngestRequest =
  | (IngestRequestBase & {
      category: "tv_sonarr";
      tvdb_id: number;
      release: string;
      season?: number;
      episode?: number;
    })
  | (IngestRequestBase & {
      category: "movie_radarr";
      tmdb_id: string;
    });

export type IngestState =
  | "error"
  | "missingFiles"
  | "uploading"
  | "pausedUP"
  | "queuedUP"
  | "stalledUP"
  | "checkingUP"
  | "forcedUP"
  | "allocating"
  | "downloading"
  | "metaDL"
  | "pausedDL"
  | "queuedDL"
  | "stalledDL"
  | "checkingDL"
  | "forcedDL"
  | "checkingResumeData"
  | "moving"
  | "unknown";

export type IngestStatus = {
  name?: string;
  size?: number;
  progress: number;
  dlspeed?: number;
  state: IngestState;
  save_path: string;
  content_path?: string;
  amount_left?: number;
  completed: boolean;
  eta?: number;
};

export type Ingest = {
  hash: string;
  request: IngestRequest;
  status: IngestStatus;
};

// this would be for partial
//
// the seperation needs to be core is limited in what its really doing it just gives the minimum contract no behavioral
//
// type IngestJobs = {
//   request: IngestRequest;
//   sonarr?: Series;
//   // download?: DownloadJob;
// };

export const ingest = new Map<string, Ingest>();
