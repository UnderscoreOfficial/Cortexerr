using Cortexerr.Core.Ingest;
using Cortexerr.Extended.Downloader;
using Cortexerr.Extended.Indexer;

namespace Cortexerr.Extended.DataStructures;

public sealed record RequestJob
{
    public required Ingest ingest { get; init; }
    public List<IndexerSearchJob> indexer_search_jobs { get; init; } = new();
    public List<DownloadJob> download_jobs { get; init; } = new();
}

// public class DownloaderJobs
// {
//     public required List<DownloadJob> download_job { get; init; } = new();
// }
