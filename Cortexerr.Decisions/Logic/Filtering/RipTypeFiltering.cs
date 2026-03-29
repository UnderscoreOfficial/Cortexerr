using Cortexerr.Core.Configuration;
using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Decisions.Logic.Filtering;

public static class RipTypeFiltering
{
    public static DecisionLogicMatchingJob Filter(DecisionLogicMatchingJob decision_logic_matching_job)
    {
        var minimum_riptype = (int)Config.ARGS.minimum_riptype;
        if (minimum_riptype > 0)
        {
            var results = decision_logic_matching_job.results.Where(
                    job => (int)job.matched.riptype.Value >= minimum_riptype).ToList();
            return new DecisionLogicMatchingJob
            {
                request_job = decision_logic_matching_job.request_job,
                search_job = decision_logic_matching_job.search_job,
                results = results
            };
        }
        return decision_logic_matching_job;
    }
}
