using Cortexerr.Core.Ingest;
using Cortexerr.Decisions.Logic;
using Cortexerr.Decisions.Orchestration;

namespace Cortexerr.Decisions.Consumer;

public class IngestConsumer(DecisionLogic logic) : IIngestConsumer
{
    // Official apps implementation 
    // RequestHandler -> Scheduler -> Sequence -> DecisionLogic + Downloader + Indexer
    public void RequestHandler(Ingest ingest)
    {
        Scheduler.RequestQueueAdd(logic, ingest);
    }
}
