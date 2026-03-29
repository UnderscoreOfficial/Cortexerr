using Cortexerr.Extended.DataStructures;
using Cortexerr.Extended.Indexer;

namespace Cortexerr.Decisions.DataStructures;

public sealed record DecisionLogicJob
{
    public required RequestJob request_job { get; init; }
    public required IndexerSearchJob search_job { get; init; }
    public required List<IndexerSearchResultItem> results { get; init; }
}
