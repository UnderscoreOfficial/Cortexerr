using Cortexerr.Core.Ingest;
using Cortexerr.Decisions.Logic;
using Cortexerr.Extended.DataStructures;
using Cortexerr.Extended.Indexer;

namespace Cortexerr.Decisions.Sequence;

public class Sequence(DecisionLogic logic, Ingest ingest)
{
    public void Radarr()
    {
        var request_job = new RequestJob
        {
            ingest = ingest
        };

        var indexer = new Indexer();
        var search_job = indexer.Search(request_job);

        // temp example structure below not real methods

        logic.Example();
        var ranked_results = new List<string>();

        foreach (var ranked in ranked_results)
        {

        }
    }
}
