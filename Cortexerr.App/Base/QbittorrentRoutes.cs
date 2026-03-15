using System.Globalization;
using System.Text.Json;
using Cortexerr.App.Decisions;
using Cortexerr.Core;
using Cortexerr.Core.Arrs;
using Cortexerr.Core.Configuration;
using MonoTorrent.BEncoding;

namespace Cortexerr.App.Base;

public static class Requested
{
    public static Dictionary<string, Ingest> ingest = new();
}

public record EncodedTorrent(
    string hash,
    int id,
    int? tvdbid,
    int? tmdbid,
    int? season,
    int? episode,
    string name,
    string release,
    long length
);

public static class QbittorrentRoutes
{
    private static IngestConsumer _ingest_consumer => new();

    private static int? TorrentDecodeInt(BEncodedDictionary dict, string key)
    {
        if (!dict.TryGetValue((BEncodedString)key, out var value))
            return null;
        return (int)((BEncodedNumber)value).Number;
    }
    async public static Task<IResult> TorrentAdd(HttpRequest request)
    {
        if (!request.HasFormContentType)
            return Results.BadRequest("(QbittorrentRoutes|TorrentAdd) Expected multipart form data");
        var form = await request.ReadFormAsync();

        var file = form.Files["torrents"];
        if (file == null)
            return Results.BadRequest("(QbittorrentRoutes|TorrentAdd) Missing torrent file parts");

        byte[] data;
        using (var memory_stream = new MemoryStream())
        {
            await file.CopyToAsync(memory_stream);
            data = memory_stream.ToArray();
        }

        var root = (BEncodedDictionary)BEncodedValue.Decode(data);
        var info = (BEncodedDictionary)root[(BEncodedString)"info"];

        var id = TorrentDecodeInt(info, "id");
        var tvdbid = TorrentDecodeInt(info, "tvdbid");
        var tmdbid = TorrentDecodeInt(info, "tmdbid");
        var season = TorrentDecodeInt(info, "season");
        var episode = TorrentDecodeInt(info, "episode");

        var hash = info[(BEncodedString)"hash"].ToString();
        var release = info[(BEncodedString)"release"].ToString();

        var length_string = info[(BEncodedString)"length"].ToString();
        if (!double.TryParse(length_string, NumberStyles.Float, CultureInfo.InvariantCulture, out var length))
            throw new InvalidDataException($"Invalid double for 'length': '{length_string}'");

        if (string.IsNullOrWhiteSpace(hash))
            return Results.BadRequest("(QbittorrentRoutes|TorrentAdd) Invalid hash");
        if (string.IsNullOrWhiteSpace(release))
            return Results.BadRequest("(QbittorrentRoutes|TorrentAdd) Invalid release");
        if (!id.HasValue)
            return Results.BadRequest("(QbittorrentRoutes|TorrentAdd) Invalid id");


        Ingest? ingest = null;
        if (tvdbid.HasValue)
        {
            var series = await Sonarr.Series(tvdbid.Value);
            if (series.error != null)
                return Results.BadRequest("(QbittorrentRoutes|TorrentAdd) Could not fetch series data");
            if (series.data == null)
                return Results.BadRequest("(QbittorrentRoutes|TorrentAdd) Series no error and missing data");
            var name = $"{series.data.title_slug?.Replace("-", ".").Replace(" ", ".")}.{release}";
            var status = new IngestStatus
            {
                name = name,
                size = length,
                progress = 0,
                download_speed = 0,
                state = TorrentState.UNKNOWN,
                save_path = "/",
                // content_path
                eta = 0,
                completed = false
            };
            var ingest_sonarr_request = new IngestSonarrRequest
            {
                rid = id.Value,
                tvdb_id = tvdbid.Value,
                length = length,
                release = release,
                season = season,
                episode = episode
            };
            var ingest_sonarr = new IngestSonarr
            {
                request = ingest_sonarr_request,
                series = series.data

            };
            ingest = new Ingest
            {
                hash = hash,
                sonarr = ingest_sonarr,
                status = status
            };
        }
        else if (tmdbid.HasValue)
        {
            var movie = await Radarr.Movie(tmdbid.Value);
            if (movie.error != null)
                return Results.BadRequest("(QbittorrentRoutes|TorrentAdd) Could not fetch movie data");
            if (movie.data == null)
                return Results.BadRequest("(QbittorrentRoutes|TorrentAdd) Movie no error and missing data");
            var name = $"{movie.data.sort_title?.Replace("-", ".").Replace(" ", ".")}.{release}";
            var status = new IngestStatus
            {
                name = name,
                size = length,
                progress = 0,
                download_speed = 0,
                state = TorrentState.UNKNOWN,
                save_path = "/",
                // content_path
                eta = 0,
                completed = false
            };
            var ingest_radarr_request = new IngestRadarrRequest
            {
                rid = id.Value,
                tmdb_id = tmdbid.Value,
                length = length,
            };
            var ingest_radarr = new IngestRadarr
            {
                request = ingest_radarr_request,
                movie = movie.data

            };
            ingest = new Ingest
            {
                hash = hash,
                radarr = ingest_radarr,
                status = status
            };
        }
        if (ingest != null)
        {
            Requested.ingest.Add(ingest.hash, ingest);
            Console.WriteLine(JsonSerializer.Serialize(ingest));
            // this needs to support external dll and default logic
            _ingest_consumer.RequestHandler(ingest);
            return Results.Ok();
        }
        return Results.BadRequest("(QbittorrentRoutes|TorrentAdd) Unknown error failed to add");
    }

    public static WebApplication MapQbittorrentRoutes(this WebApplication app)
    {
        app.MapPost("/downloader/api/v2/torrents/add", async (HttpRequest request) =>
        {
            return await TorrentAdd(request);
        });
        app.MapPost("/downloader/api/v2/torrents/delete", async (HttpRequest request) =>
        {
            var form = await request.ReadFormAsync();
            var hashes = form["hashes"].ToString().Split("|");

            if (hashes.Length == 0)
                return Results.BadRequest("(QbittorrentRoutes|TorrentDelete) Missing hashes");

            foreach (var hash in hashes)
            {
                // temporary need to trigger a flag for removal or something
                Requested.ingest.Remove(hash);
            }

            return Results.Ok();
        });
        app.MapGet("/downloader/api/v2/torrents/info", (string category) =>
        {
            if (!(category == "tv_sonarr" || category == "movie_radarr"))
                return Results.BadRequest("(QbittorrentRoutes|TorrentInfo) Invalid category");

            var info = new List<Object>();
            foreach (var ingest in Requested.ingest.Values)
            {
                double? amount_left = null;
                if (!ingest.status.completed && ingest.status.size > 0)
                {
                    amount_left = ingest.status.size * (1 - ingest.status.progress);
                }
                var name = ingest.status.name.Replace("-", ".").Replace(" ", ".");
                info.Add(new
                {
                    hash = ingest.hash,
                    progress = ingest.status.progress,
                    dlspeed = ingest.status.download_speed,
                    state = ingest.status.state,
                    category = category,
                    save_path = ingest.status.save_path,
                    content_path = ingest.status.content_path,
                    completion_on = ingest.status.completed ? DateTimeOffset.UtcNow.ToUnixTimeSeconds() : 0,
                    eta = ingest.status.eta,
                    amount_left = amount_left != null ? amount_left : 0,
                    size = ingest.status.size,
                    name = name
                });
            }
            return Results.Ok(info);
        });
        app.MapGet("/downloader/api/v2/app/webapiVersion", () =>
        {
            return Results.Text("2.8.3");
        });
        app.MapGet("/downloader/api/v2/app/preferences", () =>
        {
            var preferences = new
            {
                temp_path_enabled = false,
                temp_path = "",
                scan_dirs = new { },
                auto_tmm_enabled = false,
                torrent_content_layout = "Original",
                start_paused_enabled = false,
                auto_delete_mode = 0,
                preallocate_all = false,
                incomplete_files_ext = false,
                web_ui_username = "admin",
                save_resume_data_interval = 15,
            };
            return Results.Ok(preferences);
        });
        app.MapGet("/downloader/api/v2/torrents/categories", () =>
        {
            var categories = new
            {
                tv_sonarr = new { savePath = Config.ARGS.sonarr_download_path },
                movie_radarr = new { savePath = Config.ARGS.radarr_download_path },
            };
            return Results.Ok(categories);
        });

        return app;
    }
}

