import type { NormalizedTorrent } from "@ctrl/shared-torrent";
type SabnzbdQueueResponse = {
  queue: {
    status: string;
    speedlimit: string;
    speedlimit_abs: string;
    paused: boolean;
    noofslots_total: number;
    noofslots: number;
    limit: number;
    start: number;
    timeleft: string;
    speed: string;
    kbpersec: string;
    size: string;
    sizeleft: string;
    mb: string;
    mbleft: string;
    slots: {
      status: string;
      index: number;
      password: string;
      avg_age: string;
      time_added: number;
      script: string;
      direct_unpack: string | null;
      mb: string;
      mbleft: string;
      mbmissing: string;
      size: string;
      sizeleft: string;
      filename: string;
      labels: string[];
      priority: string;
      cat: string;
      timeleft: string;
      percentage: string;
      nzo_id: string;
      unpackopts: string;
    }[];
    diskspace1: string;
    diskspace2: string;
    diskspacetotal1: string;
    diskspacetotal2: string;
    diskspace1_norm: string;
    diskspace2_norm: string;
    have_warnings: string;
    pause_int: string;
    left_quota: string;
    version: string;
    finish: number;
    cache_art: string;
    cache_size: string;
    finishaction: string | null;
    paused_all: boolean;
    quota: string;
    have_quota: boolean;
  };
};
export declare class RDTClient {
  #private;
  magnet: string;
  hash: string;
  constructor(magnet: string, hash?: string);

  /**
   * Fetches torrent data by hash.
   *
   * Designed for use in a user‑controlled polling loop, where the caller determines
   * when to stop polling, handle errors, remove torrents, or retry downloads.
   *
   * In default polling mode, the function automatically waits between requests using
   * a built‑in adaptive delay policy based on torrent progress:
   * - Fast polling during startup and near completion.
   * - Slower intervals during the main download phase.
   *
   * This adaptive timing provides "sane default" cadence for progress checks,
   * but **all state‑handling decisions** (e.g., canceling stalled torrents,
   * retrying failed downloads, or finalizing on completion) are left entirely
   * to the caller’s logic.
   *
   * @param polling - Whether to apply the adaptive polling delay. Defaults to `true`.
   *                  Set to `false` to disable internal delays for manual timing control.
   * @returns NormalizedTorrent - An object representing the torrent data.
   *
   * @example
   * ```ts
   * // Example of user‑controlled polling:
   * while (true) {
   *   const client = new Qbittorrent("magnet:example");
   *   const torrent = await client.getTorrent();
   *   if (torrent.progress >= 100 || torrent.downloadSpeed == 0) break;
   * }
   * ```
   */
  getTorrent(polling?: boolean): Promise<NormalizedTorrent>;
  removeTorrent(delete_files?: boolean): Promise<void>;
  addTorrent(): Promise<NormalizedTorrent>;
}
export declare class Sabnzbd {
  #private;
  nzb_link: string;
  nzo_id: string;
  constructor(nzb_link: string);
  queue(polling?: boolean): Promise<{
    sabnzbd: SabnzbdQueueResponse;
    nzb: {
      status: string;
      index: number;
      password: string;
      avg_age: string;
      time_added: number;
      script: string;
      direct_unpack: string | null;
      mb: string;
      mbleft: string;
      mbmissing: string;
      size: string;
      sizeleft: string;
      filename: string;
      labels: string[];
      priority: string;
      cat: string;
      timeleft: string;
      percentage: string;
      nzo_id: string;
      unpackopts: string;
    };
  }>;
  addNzb(): Promise<void>;
  deleteNzb(delete_files?: boolean): Promise<void>;
  pauseNzb(): Promise<void>;
  resumeNzb(): Promise<void>;
}
export {};
