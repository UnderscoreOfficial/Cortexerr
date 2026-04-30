using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Decisions.Logic.Filtering;

public static class Filtering
{
    public static DecisionLogicMatchingJob Filter(DecisionLogicMatchingJob decision_logic_matching_job)
    {
        var request_filtering = RequestFiltering.Filter(decision_logic_matching_job);
        var keyword_filtering = KeywordFiltering.Filter(request_filtering);
        var riptype_filtering = RipTypeFiltering.Filter(keyword_filtering);
        var size_filtering = SizeFiltering.Filter(riptype_filtering);
        var high_dynamic_range_dolby_vision_filtering = HighDynamicRangeDolbyVisionFiltering.Filter(size_filtering);
        var resolution_filtering = ResolutionFiltering.Filter(high_dynamic_range_dolby_vision_filtering);
        return resolution_filtering;
    }
}
