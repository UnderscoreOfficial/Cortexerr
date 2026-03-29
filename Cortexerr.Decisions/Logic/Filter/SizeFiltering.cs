using Cortexerr.Core.Configuration;
using Cortexerr.Decisions.DataStructures;

namespace Cortexerr.Decisions.Logic.Filter;

public static class SizeMatching
{
    public static DecisionLogicJob Filter(DecisionLogicJob decision_logic_job)
    {
        if (Config.ARGS.download_max_size > 0)
        {
            var results = decision_logic_job.results.Where(item => item.size <= Config.ARGS.download_max_size).ToList();
            return new DecisionLogicJob
            {
                request_job = decision_logic_job.request_job,
                search_job = decision_logic_job.search_job,
                results = results
            };
        }
        return decision_logic_job;
    }
}
