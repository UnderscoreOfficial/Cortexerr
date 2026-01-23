import * as dotenv from "dotenv";
import { parseArgs } from "node:util";
import os from "os";
import pino from "pino";
dotenv.config();

// logger
export const logger = pino({
  level: "info",
  base: {
    pid: process.pid,
  },
  timestamp: pino.stdTimeFunctions.isoTime,
  transport: {
    target: "pino-pretty",
    options: {
      colorize: true,
    },
  },
});

const services = [
  "sonarr",
  "radarr",
  "jackett",
  "hydra",
  "rdtclient",
  "sabnzbd",
] as const;
type Services = Record<(typeof services)[number], URL>;

type FormattedArgs = Services & {
  host: URL;
  interface: string;
};

type Args = Omit<typeof parsed_args.values, keyof FormattedArgs> &
  FormattedArgs;

// default constant references
export const CONST = {
  ADDRESS: "http://127.0.0.1",
  HOST_PORT: "8989",

  // sonarr cannot have the same port as proxy best to change off default so other services still work properly
  SONARR_PORT: "8989",
  SONARR_API_KEY: process.env["SONARR_API_KEY"],

  RADARR_PORT: "7878",
  RADARR_API_KEY: process.env["RADARR_API_KEY"],

  // Hydra is treated as usenet only! if torrents are included it will break things
  // jackett is treated as torrents only

  // [first class] torrent only
  JACKETT_PORT: "9117",
  JACKETT_API_KEY: process.env["JACKETT_API_KEY"],

  // [first class] usenet only
  HYDRA_PORT: "5076",
  HYDRA_API_KEY: process.env["HYDRA_API_KEY"],

  // [first class] debrid (torrents) only
  RDTCLIENT_PORT: "6500",
  RDTCLIENT_USERNAME: process.env["RDTCLIENT_USERNAME"],
  RDTCLIENT_PASSWORD: process.env["RDTCLIENT_PASSWORD"],

  // [first class] usenet only
  SABNZBD_PORT: "8080",
  SABNZBD_API_KEY: process.env["SABNZBD_API_KEY"],
} as const;

// args
const parsed_args = parseArgs({
  options: {
    debug: {
      type: "boolean", // only enable for debuging
      default: false,
    },
    host: {
      type: "string", // poxy - host:ip format
    },
    sonarr: {
      type: "string", // sonarr - host:ip format
      default: `${CONST.ADDRESS}:${CONST.SONARR_PORT}`,
    },
    radarr: {
      type: "string", // radarr - host:ip format
      default: `${CONST.ADDRESS}:${CONST.RADARR_PORT}`,
    },
    jackett: {
      type: "string", // jackett - host:ip format
      default: `${CONST.ADDRESS}:${CONST.JACKETT_PORT}`,
    },
    hydra: {
      type: "string", // hydra - host:ip format
      default: `${CONST.ADDRESS}:${CONST.HYDRA_PORT}`,
    },
    rdtclient: {
      type: "string", // rdtclient - host:ip format
      default: `${CONST.ADDRESS}:${CONST.RDTCLIENT_PORT}`,
    },
    sabnzbd: {
      type: "string", // sabnzbd - host:ip format
      default: `${CONST.ADDRESS}:${CONST.SABNZBD_PORT}`,
    },
    // must be identical for all downloaders or mount the same local directories
    sonarr_download_path: {
      type: "string", // /shared/download/path without any trailing /
      default: `/sonarr`,
    },
    radarr_download_path: {
      type: "string",
      default: `/radarr`,
    },
    rss_sync: {
      type: "boolean",
      default: false,
    },
    rss_sync_interval: {
      type: "string",
      default: "60",
    },
  },
  allowPositionals: true,
});

function validateEnvs() {
  const errors = [];
  if (!CONST.SONARR_API_KEY) {
    errors.push("Sonarr Api key missing!");
  }
  if (!CONST.RADARR_API_KEY) {
    throw new Error("Radarr Api key missing");
  }
  if (!CONST.HYDRA_API_KEY && !CONST.JACKETT_API_KEY) {
    errors.push("Hydra & Jackett Api key missing, at least one is required!");
  }
  if (
    !CONST.SABNZBD_API_KEY &&
    !CONST.RDTCLIENT_PASSWORD &&
    !CONST.RDTCLIENT_USERNAME
  ) {
    errors.push(
      "Sabnzbd Api key & Debrids login credentials are missing, at least one is required!",
    );
  }
  if (errors.length) {
    throw new Error(JSON.stringify(errors));
  }
}
validateEnvs();

function formatArgs() {
  const formatted_args = {} as FormattedArgs;

  for (let service of services) {
    formatted_args[service] = parseAddress(
      parsed_args.values[service],
      service,
    );
  }

  const host = parsed_args.values.host;
  if (host) {
    formatted_args.host = parseAddress(host, "host");
  } else {
    const local_host = getLocalHost();
    formatted_args.host = local_host
      ? parseAddress(local_host.address, "host")
      : new URL(CONST.ADDRESS);
    formatted_args.interface = local_host?.interface
      ? local_host.interface
      : "unknown";
  }
  return formatted_args;
}

function parseAddress(address: string, service: string) {
  const trimmed_address = address.trim();
  const has_http = /^https?:\/\//i.test(trimmed_address);

  let new_url = has_http
    ? new URL(trimmed_address)
    : new URL(`http://${trimmed_address}`);

  if (!new_url.port) {
    const port_key = `${service.toUpperCase()}_PORT` as keyof typeof CONST;
    const port = CONST[port_key];
    if (port) {
      new_url.port = port;
    }
  }

  return new_url;
}

function getLocalHost() {
  const interfaces = os.networkInterfaces();
  for (const [name, addrs] of Object.entries(interfaces)) {
    for (const addr of addrs ?? []) {
      if (addr.family === "IPv4" && !addr.internal) {
        return { interface: name, address: addr.address };
      }
    }
  }
}
const formatted_args = formatArgs();

export const args: Args = {
  ...parsed_args.values,
  ...formatted_args,
};
