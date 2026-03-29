using Cortexerr.Decisions.DataStructures;

namespace Cortexerr.Decisions.Logic.Filter;

public static class DuplicatesFiltering
{
    public static DecisionLogicJob Filter(DecisionLogicJob decision_logic_job)
    {
        var results = decision_logic_job.results.DistinctBy(item => item.name).ToList();
        return new DecisionLogicJob
        {
            request_job = decision_logic_job.request_job,
            search_job = decision_logic_job.search_job,
            results = results
        };
    }
}
