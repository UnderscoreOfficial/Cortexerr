// import "./core/args.js";
import "@cortexerr/core/args.js";

// import { args, Qbittorrent, Sabnzbd } from "@cortexerr/core";

import "./old/sonarr-proxy.js";
import "./base/indexer-proxy.js";
import "./base/downloader-proxy.js";

import { args, Debrid, Jackett, logger, Utils } from "@cortexerr/core";

console.log(args);

// real case will be responsive not reative like this (ie) ill be able to directly trigger like a update function after I add a new item to utils
// rather than constantly checking it 24/7
// function updateIngest(
//   progress: number,
//   state: string,
//   name?: string,
//   completed: boolean = false,
// ) {
//   if (ingest_requests.size > 0) {
//     for (const [hash, ingest] of ingest_requests) {
//       ingest.name =
//         name ?? "Arcane.S02.1080p.ITA-ENG.BluRay.x265.AAC-V3SP4EV3R";
//       ingest.progress = progress;
//       ingest.state = state;
//       ingest.save_path =
//         "/sonarr/Arcane.S02.1080p.ITA-ENG.BluRay.x265.AAC-V3SP4EV3R";
//       ingest.completed = completed;
//       ingest.size = 5690832000; // bytes
//     }
//     logger.info(ingest_requests);
//   }
// }
//
// async function checkIngest() {
//   await Utils.delay(30);
//   updateIngest(0.3, "downloading");
//
//   await Utils.delay(30);
//   updateIngest(
//     0.62,
//     "downloading",
//     "Arcane.S02.1080p.ITA-ENG.Remux.H265.HVEC-V3SP4EV3R.#2",
//   );
//
//   await Utils.delay(30);
//   updateIngest(
//     0.3,
//     "downloading",
//     "Arcane.S02.1080p.ITA-ENG.Remux.H265.HVEC-V3SP4EV3R",
//   );
//
//   await Utils.delay(30);
//   updateIngest(0.62, "downloading");
//
//   await Utils.delay(30);
//   updateIngest(
//     0.15,
//     "downloading",
//     "Arcane.S02.1080p.ITA-ENG.Remux.H265.HVEC-V3SP4EV3R",
//   );
//
//   await Utils.delay(30);
//   updateIngest(
//     0.15,
//     "downloading",
//     "Arcane.S02.1080p.ITA-ENG.Remux.H265.HVEC-V3SP4EV3R",
//   );
//
//   // await Utils.delay(20);
//   // updateIngest(1, "stalledUP", true);
// }
// checkIngest();

// this is a fake example of what a real queued items response might be like

// const sab = new Sabnzbd(
//   "http://192.168.1.117:5076/getnzb/api/1937862949612197060?apikey=137BOR16NFFQRDMOU4O1Q6PE2U",
// );
// const data = sab.addNzb();
// const que = await sab.queue();
// console.log(que.sabnzbd);
// console.log(que.nzb);

// const data = sab.addNzb();
// console.log(data);
// await sab.pauseNzb();
// await sab.resumeNzb();
// await sab.deleteNzb();

// const qbit = new Qbittorrent(
//   "magnet:?xt=urn:btih:4B91C9289FD752522348D5A67A4E8157DD1B5DF2&dn=Rick.and.Morty.S01.1080p.BluRay.x265&tr=udp%3A%2F%2Ftracker.opentrackr.org%3A1337%2Fannounce&tr=udp%3A%2F%2Fopen.tracker.cl%3A1337%2Fannounce&tr=udp%3A%2F%2Fopen.demonii.com%3A1337%2Fannounce&tr=udp%3A%2F%2Fopen.stealth.si%3A80%2Fannounce&tr=udp%3A%2F%2Ftracker.torrent.eu.org%3A451%2Fannounce&tr=udp%3A%2F%2Fexodus.desync.com%3A6969%2Fannounce&tr=udp%3A%2F%2Fopen.dstud.io%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.ololosh.space%3A6969%2Fannounce&tr=udp%3A%2F%2Fexplodie.org%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker-udp.gbitt.info%3A80%2Fannounce",
//   "4B91C9289FD752522348D5A67A4E8157DD1B5DF2",
// );
// const data = await qbit.addTorrent();
// console.log(data);

// const r = await qbit.getTorrent();
// console.log(r);
//
// console.log(qbit.hash);
// while (true) {
//   const r = await qbit.getTorrent();
//   console.log(r);
// }

// await qbit.stopTorrent();
// await qbit.startTorrent();
// await qbit.removeTorrent();

// const results = await Jackett.tvSearch({
//   tvdb_id: "371028",
//   query: "arcane",
//   season: 2,
// });

// const results = await Hydra.tvSearch({
//   tvdb_id: "275274",
//   season: 1,
// });
// console.log(JSON.stringify(results?.json));
// const item = results?.json[99];
//
// logger.info(item);
// logger.info(`${Number(item?.size) / 1000000 / 1000} GB`);

// if (!results) throw Error("no results");
// for (let torrent of results.json) {
//   const values = torrent["torznab:attr"];
//   if (values) {
//     for (let i of values) {
//       if (i.name == "seeders") {
//         i.
//       };
//     }
//   }
// }
// const mag =
//   "magnet:?xt=urn:btih:BBE86011BD0FE22BAA020FCDE6819B3C2263296B&dn=Arcane.S02.1080p.ITA-ENG.BluRay.x265.AAC-V3SP4EV3R&tr=udp%3A%2F%2Fopen.stealth.si%3A80%2Fannounce&tr=udp%3A%2F%2Ftracker.opentrackr.org%3A1337%2Fannounce&tr=udp%3A%2F%2Fexodus.desync.com%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.torrent.eu.org%3A451%2Fannounce&tr=http%3A%2F%2Fnyaa.tracker.wf%3A7777%2Fannounce&tr=udp%3A%2F%2Fmartin-gebhardt.eu%3A25%2Fannounce&tr=udp%3A%2F%2Fretracker.lanta.me%3A2710%2Fannounce&tr=udp%3A%2F%2Fttk2.nbaonlineservice.com%3A6969%2Fannounce&tr=udp%3A%2F%2Fopentracker.io%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.fnix.net%3A6969%2Fannounce&tr=udp%3A%2F%2Fd40969.acod.regrucolo.ru%3A6969%2Fannounce&tr=udp%3A%2F%2Fodd-hd.fr%3A6969%2Fannounce&tr=http%3A%2F%2Fbt.okmp3.ru%3A2710%2Fannounce&tr=http%3A%2F%2Fbvarf.tracker.sh%3A2086%2Fannounce&tr=udp%3A%2F%2Ftracker.opentrackr.org%3A1337%2Fannounce&tr=http%3A%2F%2Ftracker.openbittorrent.com%3A80%2Fannounce&tr=udp%3A%2F%2Fopentracker.i2p.rocks%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.internetwarriors.net%3A1337%2Fannounce&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969%2Fannounce&tr=udp%3A%2F%2Fcoppersurfer.tk%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.zer0day.to%3A1337%2Fannounce";
// if (item) {
//   const torrent = new Debrid(item.guid);
//   torrent.addTorrent();
// }
// const torrent = new Debrid(mag);
// torrent.addTorrent();

// const results = await Hydra.search("275274");
// console.log(results.json);
// console.log(results.json.data[0]);
