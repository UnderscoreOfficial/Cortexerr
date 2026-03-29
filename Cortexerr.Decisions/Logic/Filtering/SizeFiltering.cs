using Cortexerr.Core.Configuration;
using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Decisions.Logic.Filtering;

public static class SizeFiltering
{
    public static DecisionLogicMatchingJob Filter(DecisionLogicMatchingJob decision_logic_matching_job)
    {
        if (Config.ARGS.download_max_size > 0)
        {
            var results = decision_logic_matching_job.results.Where(job => job.item.size <= Config.ARGS.download_max_size).ToList();
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
