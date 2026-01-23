import { QBittorrent } from "@ctrl/qbittorrent";
// unused without there will be missing type warning when building for Qbittorrent.getTorrent()
import type { NormalizedTorrent } from "@ctrl/shared-torrent";
import { args, CONST, logger } from "./args.js";
import { Utils } from "./util.js";

const qb_client = new QBittorrent({
  baseUrl: args.rdtclient.toString(),
  password: String(CONST.RDTCLIENT_PASSWORD),
  username: String(CONST.RDTCLIENT_USERNAME),
});

type SabnzbAddResponse = {
  status: boolean;
  nzo_ids: string[];
};

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

// this at first seems odd im wrapping a already very well documented class the end goal here I is to its not limiting the scope of what you can do but
// focusing on a default behavior ie I want each instance of the RDTClient class as a Part of the Downloders map and further this will be doing it own
// checking behavior maybe im wrong but I actually don't think there is much api I have to passthough to allow near full control of the added torrents.
export class RDTClient {
  public magnet: string;
  public hash: string;

  constructor(magnet: string, hash?: string) {
    this.hash = hash ?? this.#getHash(magnet);
    this.magnet = magnet;
  }

  #getHash(magnet: string) {
    const match = magnet.match(/btih:([a-fA-F0-9]+)/);
    if (!match || match[1] == undefined) {
      throw new Error("Invalid magnet!");
    }
    return match[1].toUpperCase();
  }

  public async getTorrent(polling = true) {
    const torrent = await qb_client.getTorrent(this.hash);

    // idea is that the start and end want the fastest polling time
    // - start being for many the tell of some bad downloads if it has issues right away usefull to know it instantly
    // - end being prime for completion or for retrying
    if (polling) {
      if (torrent.progress <= 5 || torrent.progress >= 95) {
        await Utils.delay(5);
      } else {
        await Utils.delay(30);
      }
    }
    return torrent;
  }

  public async removeTorrent(delete_files = true) {
    await qb_client.removeTorrent(this.hash, delete_files);
    logger.info(`Torrent Removed - [${this.hash}]`);
  }

  public async addTorrent() {
    await qb_client.addMagnet(this.magnet);
    logger.info(`Torrent Added - [${this.hash}]`);
    return this.getTorrent(false);
  }
}

// sabnzbd api v4.5
export class Sabnzbd {
  public nzb_link: string;
  public nzo_id: string;

  constructor(nzb_link: string) {
    this.nzb_link = nzb_link;
    this.nzo_id = "";
  }

  async #request(api: string) {
    const url = `${args.sabnzbd.origin}/api?output=json&apikey=${CONST.SABNZBD_API_KEY}&${api}`;
    console.log(url);
    const response = await fetch(url);
    return response.json();
  }

  public async queue(polling = true) {
    const api = `mode=queue&nzo_ids=${this.nzo_id}`;
    const data = (await this.#request(api)) as SabnzbdQueueResponse;
    const nzb = data.queue.slots[0];
    if (!nzb) throw new Error("N");
    if (polling) {
      if (Number(nzb.percentage) <= 5 || Number(nzb.percentage) >= 95) {
        await Utils.delay(5);
      } else {
        await Utils.delay(20);
      }
    }
    return { sabnzbd: data, nzb };
  }

  public async addNzb() {
    const api = `mode=addurl&name=${encodeURIComponent(this.nzb_link)}&nzbname=&script=Default&priority=-100&pp=-1`;
    const data = (await this.#request(api)) as SabnzbAddResponse;
    if (data.nzo_ids[0]) {
      this.nzo_id = data.nzo_ids[0];
    } else {
      throw new Error("Missing nzo id!");
    }
  }

  public async deleteNzb(delete_files = true) {
    let api = `mode=queue&name=delete&value=${this.nzo_id}`;
    if (delete_files) {
      api += "&del_files=1";
    }
    const data = await this.#request(api);
    console.log(data);
  }

  public async pauseNzb() {
    const api = `mode=queue&name=pause&value=${this.nzo_id}`;
    await this.#request(api);
    logger.info(`Nzb pausing - [${this.nzo_id}]`);
  }

  public async resumeNzb() {
    const api = `mode=queue&name=resume&value=${this.nzo_id}`;
    await this.#request(api);
    logger.info(`Nzb resuming - [${this.nzo_id}]`);
  }
}
