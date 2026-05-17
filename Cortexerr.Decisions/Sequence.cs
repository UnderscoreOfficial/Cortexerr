using System.Text.Json;
using Cortexerr.Core.Configuration;
using Cortexerr.Core.Errors;
using Cortexerr.Core.Ingest;
using Cortexerr.Core.Logging;
using Cortexerr.Decisions.Logic;
using Cortexerr.Decisions.Logic.Filtering;
using Cortexerr.Decisions.Logic.Matching;
using Cortexerr.Decisions.Logic.Sorting;
using Cortexerr.Extended.DataStructures;
using Cortexerr.Extended.Downloader;
using Cortexerr.Extended.Indexer;
using Renci.SshNet;

namespace Cortexerr.Decisions.Orchestration;

public static class Sequence
{
    async private static Task Sonarr(DecisionLogic logic, RequestJob request_job)
    {
        var indexer = new Indexer();
        var client = new SshClient(Config.ARGS.tmp_move_remote_ip, Config.ARGS.tmp_move_remote_user, new
                PrivateKeyFile(Config.ARGS.tmp_move_id_rsa_path));
        client.Connect();
        var sonarr = request_job.ingest.sonarr;
        var series_path = $"/{sonarr?.series.title_slug?.Replace(" ", ".")}.season.{sonarr?.request.season}.cortexerr";
        var download_path = $"{Config.ARGS.sonarr_path}{series_path}";
        client.RunCommand($"mkdir -p {download_path}");
        request_job.ingest.status.content_path = $"{Config.ARGS.sonarr_download_path}{series_path}";

        while (true)
        {
            var error_message_details =
                $"""
                 - Failed <tvdbid:{request_job.ingest.sonarr?.request?.tvdb_id}>
                 <season:{request_job.ingest.sonarr?.request?.season}>
                 <ep:{request_job.ingest.sonarr?.request?.episode}>
                """;

            if (sonarr == null)
            {
                Logger.Log.Error($"(Sequence|Sonarr) {ErrorCode.UNEXPECTED_ERROR.ToString()}{error_message_details}");
                client.Disconnect();
                Requested.ingest.Remove(request_job.ingest.hash);
                return;
            }

            var search_job = await indexer.Search(request_job);
            if (search_job.error != null)
            {
                Logger.Log.Error($"(Sequence|Sonarr) {search_job.error.code.ToString()}{error_message_details}");
                client.Disconnect();
                Requested.ingest.Remove(request_job.ingest.hash);
                return;
            }
            if (search_job.data == null)
            {
                Logger.Log.Error($"(Sequence|Sonarr) {ErrorCode.UNEXPECTED_ERROR.ToString()}{error_message_details}");
                client.Disconnect();
                Requested.ingest.Remove(request_job.ingest.hash);
                return;
            }
            // temp will be moved to decision logic mappings
            // search_job.data.indexer_search_job
            var matched = Matching.Match(request_job, search_job.data);
            var decision_logic_job = Filtering.Filter(matched);
            // Console.WriteLine(JsonSerializer.Serialize(decision_logic_job.results.Select(i => new { name = i.item.name, size = i.item.size / 1000000000.00 })));
            decision_logic_job.results.Sort(new Sorting(decision_logic_job));
            Console.WriteLine(JsonSerializer.Serialize(decision_logic_job.results.Select(i => new { name = i.item.name, size = i.item.size / 1000000000.00, date = i.item.upload_date })));

            var download_job = await Downloader.Download(decision_logic_job.request_job, decision_logic_job.results.Select(i => i.item).ToList());
            if (download_job.error != null)
            {
                Logger.Log.Error($"(Sequence|Sonarr) {download_job.error.ToString()}{error_message_details}");
                request_job.ingest.status.state = TorrentState.ERROR;
                client.Disconnect();
                Requested.ingest.Remove(request_job.ingest.hash);
                return;
            }
            if (download_job.data == null)
            {
                Logger.Log.Error($"(Sequence|Sonarr) {ErrorCode.UNEXPECTED_ERROR.ToString()}{error_message_details}");
                request_job.ingest.status.state = TorrentState.ERROR;
                client.Disconnect();
                Requested.ingest.Remove(request_job.ingest.hash);
                return;
            }
            if (download_job.data.sabnzbd_state?.data?.history?.storage != null)
            {
                var storage = download_job.data.sabnzbd_state?.data?.history?.storage?.Replace("/downloads/", $"{Config.ARGS.sabnzbd_path}/");
                Logger.Log.Information($"(Sequence|Sonarr) Moving {sonarr.series.title_slug}");
                Logger.Log.Information($"(Sequence|Sonarr) -> {storage}");
                var sabnzbd_move = client.RunCommand($"mv {storage} {download_path}");
                if (string.IsNullOrEmpty(sabnzbd_move.Error))
                    Logger.Log.Error($"(Sequence|Sonarr) Failed to move {sonarr.series.title_slug}");
            }
            if (download_job.data.rdtclient_state?.data?.content_path != null)
            {
                Logger.Log.Information($"(Sequence|Sonarr) Moving {sonarr.series.title_slug}");
                var sabnzbd_move = client.RunCommand($"mv {download_job.data.rdtclient_state?.data?.content_path} {download_path}");
                if (string.IsNullOrEmpty(sabnzbd_move.Error))
                    Logger.Log.Error($"(Sequence|Sonarr) Failed to move {sonarr.series.title_slug}");
            }
            if (search_job.data.finished)
            {
                Logger.Log.Information($"(Sequence|Sonarr) Successfully downloaded ({download_job.data.item.name})");
                request_job.ingest.status.state = TorrentState.STALLED_UP;
                request_job.ingest.status.progress = 1.00f;
                client.Disconnect();
                return;
            }
        }
    }

    async private static Task Radarr(DecisionLogic logic, RequestJob request_job)
    {
        var indexer = new Indexer();
        var client = new SshClient(Config.ARGS.tmp_move_remote_ip, Config.ARGS.tmp_move_remote_user, new
                PrivateKeyFile(Config.ARGS.tmp_move_id_rsa_path));
        client.Connect();
        var radarr = request_job.ingest.radarr;
        var movie_path = $"/{radarr?.movie.original_title?.ToLower()?.Replace(" ", ".")}.{radarr?.movie.year}.cortexerr";
        var download_path = $"{Config.ARGS.radarr_path}{movie_path}";
        client.RunCommand($"mkdir -p {download_path}");

        var error_message_details =
            $"""
                 - Failed <tmdbid:{request_job.ingest.radarr?.request?.tmdb_id}>
                """;

        if (radarr == null)
        {
            Logger.Log.Error($"(Sequence|Radarr) {ErrorCode.UNEXPECTED_ERROR.ToString()}{error_message_details}");
            client.Disconnect();
            Requested.ingest.Remove(request_job.ingest.hash);
            return;
        }

        var search_job = await indexer.Search(request_job);
        if (search_job.error != null)
        {
            Logger.Log.Error($"(Sequence|Radarr) {search_job.error.code.ToString()}{error_message_details}");
            client.Disconnect();
            Requested.ingest.Remove(request_job.ingest.hash);
            return;
        }
        if (search_job.data == null)
        {
            Logger.Log.Error($"(Sequence|Radarr) {ErrorCode.UNEXPECTED_ERROR.ToString()}{error_message_details}");
            client.Disconnect();
            Requested.ingest.Remove(request_job.ingest.hash);
            return;
        }
        // temp will be moved to decision logic mappings
        // search_job.data.indexer_search_job
        var matched = Matching.Match(request_job, search_job.data);
        var decision_logic_job = Filtering.Filter(matched);
        // Console.WriteLine(JsonSerializer.Serialize(decision_logic_job.results.Select(i => new { name = i.item.name, size = i.item.size / 1000000000.00 })));
        decision_logic_job.results.Sort(new Sorting(decision_logic_job));
        // Console.WriteLine(JsonSerializer.Serialize(matched));
        Console.WriteLine(JsonSerializer.Serialize(decision_logic_job.results.Select(i => new { name = i.item.name, size = i.item.size / 1000000000.00, date = i.item.upload_date })));

        var download_job = await Downloader.Download(decision_logic_job.request_job, decision_logic_job.results.Select(i => i.item).ToList());
        if (download_job.error != null)
        {
            Logger.Log.Error($"(Sequence|Radarr) {download_job.error.ToString()}{error_message_details}");
            request_job.ingest.status.state = TorrentState.ERROR;
            client.Disconnect();
            Requested.ingest.Remove(request_job.ingest.hash);
            return;
        }
        if (download_job.data == null)
        {
            Logger.Log.Error($"(Sequence|Radarr) {ErrorCode.UNEXPECTED_ERROR.ToString()}{error_message_details}");
            request_job.ingest.status.state = TorrentState.ERROR;
            client.Disconnect();
            Requested.ingest.Remove(request_job.ingest.hash);
            return;
        }
        if (download_job.data.sabnzbd_state?.data?.history?.storage != null)
        {
            var storage = download_job.data.sabnzbd_state?.data?.history?.storage?.Replace("/downloads/", $"{Config.ARGS.sabnzbd_path}/");
            Logger.Log.Information($"(Sequence|Radarr) Moving {radarr.movie.sort_title}");
            Logger.Log.Information($"(Sequence|Radarr) -> {storage}");
            var sabnzbd_move = client.RunCommand($"mv {storage} {download_path}");
            if (string.IsNullOrEmpty(sabnzbd_move.Error))
                Logger.Log.Error($"(Sequence|Radarr) Failed to move {radarr.movie.sort_title}");
        }
        if (download_job.data.rdtclient_state?.data?.content_path != null)
        {
            Logger.Log.Information($"(Sequence|Radarr) Moving {radarr.movie.sort_title}");
            var sabnzbd_move = client.RunCommand($"mv {download_job.data.rdtclient_state?.data?.content_path} {download_path}");
            if (string.IsNullOrEmpty(sabnzbd_move.Error))
                Logger.Log.Error($"(Sequence|Radarr) Failed to move {radarr.movie.sort_title}");
        }
        if (search_job.data.finished)
        {
            var mkv = client.RunCommand($"find {download_path} -type f -name '*.mkv' -printf '%s\\t%p\\n' | sort -rn | head -1 | cut -f2");

            if (string.IsNullOrEmpty(mkv.Error))
                Logger.Log.Error($"(Sequence|Radarr) Failed find valid .mvk file for {radarr.movie.sort_title}");
            var content_path = $"{Config.ARGS.radarr_download_path}{movie_path}{mkv.Result.Replace(download_path, "")}";
            Console.WriteLine("CONTENT_PATH: " + content_path);
            Console.WriteLine("SAVE_PATH: " + request_job.ingest.status.save_path);
            request_job.ingest.status.content_path = content_path;

            Logger.Log.Information($"(Sequence|Radarr) Successfully downloaded ({download_job.data.item.name})");
            request_job.ingest.status.state = TorrentState.STALLED_UP;
            request_job.ingest.status.progress = 1.00f;
            client.Disconnect();
            return;
        }
    }

    async public static Task Process(DecisionLogic logic, RequestJob request_job)
    {
        if (request_job.ingest.radarr != null)
        {
            await Radarr(logic, request_job);
        }
        if (request_job.ingest.sonarr != null)
        {
            await Sonarr(logic, request_job);
        }
    }
}
