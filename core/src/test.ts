import { args, logger } from "./args.js";
import { SonarrApi, RadarrApi } from "./stack-apis.js";

console.log(args);

// const data = await RadarrApi.getMovie(177572);
// logger.info(data);
// console.log(data);

// SonarrApi.getQueue();

// const results = await Jackett.tvSearch({
//   tvdb_id: "371028",
//   query: "arcane",
//   season: 2,
// });
//
// const results = await Hydra.tvSearch({
//   tvdb_id: "275274",
//   season: 1,
// });
// console.log(JSON.stringify(results?.json));
// console.log(JSON.stringify(results?.json[0]));
//
// const results = await Hydra.search("275274");
// console.log(results.json);
// console.log(results.json.data[0]);
