import { args } from "./args.js";
import { SonarrApi } from "./sonarr-api.js";

console.log(args);

SonarrApi.getQueue();

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
