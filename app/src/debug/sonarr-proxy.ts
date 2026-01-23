import express from "express";
import { createProxyMiddleware, fixRequestBody } from "http-proxy-middleware";
import { args, CONST, logger } from "@cortexerr/core";

const app = express();

const sonarr_target = args.sonarr.origin;

// more legacy code this was great for intercepting request but with the fake proxy is not needed
//
//   if (
//     req.body?.name == "SeriesSearch" ||
//     req.body?.name == "SeasonSearch" ||
//     req.body?.name == "EpisodeSearch"
//   ) {
//     ProwlarrApi.existingSeriesSearch(req.body);
//     proxy(req, res);
//   } else {
//     proxy(req, res);
//   }
// });
//

// app.post(["/api/v3/series"], express.json(), async (req, res) => {
//   logger.info("--- Indexer Search ---");
//   logger.info(`Headers: ${req.headers}`);
//   logger.info(`Body: ${req.body}`);
//
//   if (req.body?.addOptions?.searchForMissingEpisodes) {
//     req.body.addOptions.searchForMissingEpisodes = false;
//     ProwlarrApi.newSeriesSearch(req.body);
//   }
//
// app.post(["/api/v3/command"], express.json(), (req, res) => {
//   logger.info("--- Indexer Search ---");
//   logger.info(`Headers: ${req.headers}`);
//   logger.info(req.body);
//   proxy(req, res);
// });
//
// forward to sonarr

const proxy = createProxyMiddleware({
  target: sonarr_target,
  changeOrigin: true,
  xfwd: true,
  ws: true,
  autoRewrite: true,
  protocolRewrite: "http",

  on: {
    proxyReq(proxyReq, req) {
      if (req.headers.host) {
        proxyReq.setHeader("Host", req.headers.host);
      }

      fixRequestBody(proxyReq, req);
      logger.info(`[proxy_req] ${req.method} ${req.url} -> ${sonarr_target}`);
    },

    proxyRes(proxyRes, req, res) {
      // Log redirects so you can SEE why the browser would escape
      const loc = proxyRes.headers["location"];
      if (loc) {
        logger.info(`[proxy_res redirect] ${loc}`);
      }
    },

    error(err, req) {
      logger.error(`[proxy_error] ${err.message}`);
    },
  },
});

// Only parse + log body for the endpoint(s) you care about.
// Important: call next() so the request continues into app.use(proxy).
app.post("/api/v3/command", express.json(), (req, res, next) => {
  logger.info("[body] POST /api/v3/command");
  logger.info(req.body);
  next();
});

app.use(proxy);

app.listen(Number(args.host.port), args.host.hostname, () => {
  logger.info(`Reverse proxy listening on ${args.host.origin}`);
});
