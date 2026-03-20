using Cortexerr.Core.Ingest;
using Cortexerr.Decisions.Logic;

namespace Cortexerr.Decisions.Consumer;

public class IngestConsumer(DecisionLogic logic) : IIngestConsumer
{

    public void RequestHandler(Ingest ingest)
    {

    }
}
