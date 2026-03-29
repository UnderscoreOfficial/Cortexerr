using Cortexerr.Core.Errors;
using Cortexerr.Core.Logging;
using Cortexerr.Decisions.Logic;
using Cortexerr.Extended.DataStructures;
using Cortexerr.Extended.Downloader;
using Cortexerr.Extended.Indexer;

namespace Cortexerr.Decisions.Orchestration;

public static class Sequence
{
    async private static Task Sonarr(DecisionLogic logic, RequestJob request_job)
    {
        while (true)
        {
            var error_message_details =
                $"""
                 - Failed <tvdbid:{request_job.ingest.sonarr?.request?.tvdb_id}>
                 <season:{request_job.ingest.sonarr?.request?.season}>
                 <ep:{request_job.ingest.sonarr?.request?.episode}>
                """;

            var sonarr = request_job.ingest.sonarr;
            if (sonarr == null)
            {
                Logger.Log.Error($"(Sequence|Sonarr) {ErrorCode.UNEXPECTED_ERROR.ToString()}{error_message_details}");
                return;
            }

            var indexer = new Indexer();
            var search_job = await indexer.Search(request_job);
            if (search_job.error != null)
            {
                Logger.Log.Error($"(Sequence|Sonarr) {search_job.error.code.ToString()}{error_message_details}");
                return;
            }
            if (search_job.data == null)
            {
                Logger.Log.Error($"(Sequence|Sonarr) {ErrorCode.UNEXPECTED_ERROR.ToString()}{error_message_details}");
                return;
            }
            // search_job.data.indexer_search_job

            // temp example structure below not real methods
            logic.Example();

            var ranked_results = new List<IndexerSearchResultItem>();
            var download_job = await Downloader.Download(request_job, ranked_results);
            if (download_job.error != null)
            {
                Logger.Log.Error($"(Sequence|Sonarr) {download_job.error.ToString()}{error_message_details}");
                return;
            }
            if (download_job.data == null)
            {
                Logger.Log.Error($"(Sequence|Sonarr) {ErrorCode.UNEXPECTED_ERROR.ToString()}{error_message_details}");
                return;
            }


            if (search_job.data.finished)
            {
                Logger.Log.Information($"(Sequence|Sonarr) Successfully downloaded{error_message_details}");
                return;
            }
        }
    }

    async private static Task Radarr(DecisionLogic logic, RequestJob request_job)
    {
        var indexer = new Indexer();
        var search_job = await indexer.Search(request_job);

        // temp example structure below not real methods

        logic.Example();
        var ranked_results = new List<IndexerSearchResultItem>();
        var download_job = await Downloader.Download(request_job, ranked_results);
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
