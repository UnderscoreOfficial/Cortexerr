using System.Text.RegularExpressions;
using Cortexerr.Core.Configuration;
using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Decisions.Logic.Filtering;

public static class KeywordFiltering
{
    public static DecisionLogicMatchingJob Filter(DecisionLogicMatchingJob decision_logic_matching_job)
    {
        if (Config.ARGS.filtered_keywords.Length > 0)
        {
            var results = decision_logic_matching_job.results.Where(
                    job =>
                    {
                        var name = Regex.Replace(job.item.name, @"[._]", " ");
                        name = Regex.Replace(name, @"\s+", " ").Trim().ToLowerInvariant();
                        var is_match = false;
                        foreach (var keyword in Config.ARGS.filtered_keywords)
                        {
                            is_match = Regex.IsMatch(name, @"\b" + keyword.ToLowerInvariant() + @"\b");
                            if (is_match) break;
                        }
                        return !is_match;
                    }).ToList();
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
