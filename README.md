# Cortexerr
Sonarr / Radarr - Give the arr stack a lobotomy, cortexerr is a (in very early alpha) brain replacement. Cortexerr replaces Sonarr / Radarr's ranking and 
selection logic with a highly opinionated ranking algorithm. For people who want a deterministic, and direct aggresive goal oriented selections. When good
enough is not enough.

When sonarr/radarr makes any type of indexer search request it does directly too Cortexerr's Indexer which gives it fake generated data satisfiying the arrs while passing
passing required data into a generated torrent file. This torrent file the arrs will grab and then pass over to Cortexerr's Downloader which then has full 
control over any processing. 

Cortexer acts as a middle man between Sonarr / Radarr and your indexers / download clients all the while bypassing sonarrs ranking and sorting as such this is
not for everyone. This directly replaces Sonarr / Radarr logic meaning profiles, custom formats and anything else will not work. This also means limited scope
on downloaders and indexers in a few ways. First all downloaders and Indexers must go though cortexer if any others exist within Sonarr / Radarr that is not 
from Cortexerr it can break Cortexerr's functionality. The more opinionated part, as Cortexerr aims to focus on the highest quality data sources and most 
reliable data sets the scope of its reach needs to be tight and focused as such there are only 2 currently supported Indexers / Downloaders 

### Supported Indexers
- NZBHydra2
- Jackett
### Supported Downloaders
- Sabnzbd
- Qbittorrent *Designed with debrid based clients in mind, it will work with normal torrents however its recomended highly to use with debrid services only*

## Short Summary
Cortexerr turns Sonarr's / Radarr's basic “indexer + downloader” model into a controlled, opinionated pipeline. It fakes just enough to keep the arrs happy, 
while taking over the actual thinking searching smarter, picking better releases, handling retries intelligently, and avoiding redundant downloads.

## FAQ

### How does Cortexerr work? 
Cortexerr had to opperate around a very small avaliable scope the arr stack "lets" it work with the end result is it has 2 key parts that allow it to have
the level of control it has.
- A Fake Proxy Torznab Indexer (FPTI) TM. 
- A Fake Proxy Qbittorrent Downloader (FPQD) TM.

### Why such a limited scope?
I think the general idea is to eventually turn cortexerr into the platform for viewing a lot of data and treating downloaders & indexers as just that likely not
needing to go to them that offten anyways eventually. Meanwhile the reduction in scope gains and focus on indexer result quailty seemed like the right direction.

Indexers are the easier among the few tested only NZBHydra2 and Jackett checked all the boxes needed for features, quality of apis, and supported indexers
I think what Cortexerr is doing here is the focus as such narowing the scope allowing more focus on making the current supported parts stable makes more sense
than allowing more perfered indexers as with the exception of a few specific needs I think these 2 indexers should be able to cover a lot.

Downloaders less of concrete reasons more at tooling qbittorrent was only because most debrid services use this api and debrid was the goal for torrents. 
as for usenet Sabnzbd seemed like a reasonable enough pick this was really scope and tooling limiting. 

This was built with , [Typescript](https://github.com/microsoft/TypeScript)

## Screenshots

- Search for games to add.
![Search game](./assets/notifier-search-game.png)
*Uses giantbomb api, limited 10 results max per search*

- Add games your interested in.
![added game](./assets/notifier-game-added.png)
*Some games won't have a release date, games will update once they get a release date*

- Get notified when they release!
![game released](./assets/notifier-release-message-example.png)
*Releases send in selected channel selected in settings*

## Docker
- Build
    1. `git clone https://github.com/UnderscoreOfficial/game-release-notifier-bot.git`
    2. `docker build -t game-release-notifier-bot:1.0 .`
- Load
    1. Download the [latest release](https://github.com/UnderscoreOfficial/game-release-notifier-bot/releases)
    2. `docker load -i <release_name.tar>`

## Docker Compose
- docker compose example:
```
services:
  game-release-notifier-bot:
    image: game-release-notifier-bot:1.0
    container_name: game-release-notifier-bot
    volumes:
      - your_local_database_folder:/app/database
    # all enviroment variables are required
    environment:
      - API_KEY=#your_giantbomb_api_key
      - DISCORD_TOKEN=#your_discord_token
      - GUILD_ID=#your_server_id  #will be changed, only 1 server can be added.
      - CLIENT_ID=#your_client_id
    restart: always
```
- compose file should be .yaml can be named anything.
-`docker compose -f <file_name.yaml> up -d`


