using Cortexerr.Core.Configuration;
using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Decisions.Logic.Filtering;

public static class HighDynamicRangeDolbyVisionFiltering
{
    public static DecisionLogicMatchingJob Filter(DecisionLogicMatchingJob decision_logic_matching_job)
    {
        var high_dynamic_range = Config.ARGS.high_dynamic_range_hdr_allowed;
        var dolby_vision = Config.ARGS.dolby_vision_dv_allowed;

        if (!high_dynamic_range || !dolby_vision)
        {
            var results = new List<DecisionLogicRequestMatching>();
            foreach (var job in decision_logic_matching_job.results)
            {
                if (job.matched.dolby_vision && job.matched.high_dynamic_range && !dolby_vision && !high_dynamic_range) continue;
                if (job.matched.dolby_vision && !job.matched.high_dynamic_range && !dolby_vision) continue;
                if (!job.matched.dolby_vision && job.matched.high_dynamic_range && !high_dynamic_range) continue;
                results.Add(job);
            }
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
