using Cortexerr.Core.Ingest;
using Cortexerr.Extended.Indexer;

namespace Cortexerr.Extended.DataStructures;

public record RequestJob
{
    public required Ingest ingest { get; init; }
    public List<IndexerSearchJob> indexer_search_jobs { get; init; } = new();
}

public record DownloadJob
{
    public required Ingest ingest { get; init; }
}
