using Cortexerr.Core.Configuration;
using Cortexerr.Decisions.Logic.Filtering;

namespace Cortexerr.Tests.Unit.Decisions.FilteringTests;

public class HighDynamicRangeDolbyVisionFilteringTests
{
    [Fact]
    public void Filter_Hdr_Dv_Allowed()
    {
        if (!Config.ARGS.high_dynamic_range_hdr_allowed || !Config.ARGS.dolby_vision_dv_allowed)
            return;

        var data = DecisionsData.Data();
        var count = data.results.Count(r => r.matched.dolby_vision || r.matched.high_dynamic_range);
        var result = HighDynamicRangeDolbyVisionFiltering.Filter(data);

        Assert.Same(data, result);
        Assert.Equal(count, result.results.Count(r => r.matched.dolby_vision || r.matched.high_dynamic_range));
    }
    [Fact]
    public void Filter_Hdr_Disallowed_Dv_Disallowed()
    {
        if (Config.ARGS.dolby_vision_dv_allowed || Config.ARGS.high_dynamic_range_hdr_allowed)
            return;

        var result = HighDynamicRangeDolbyVisionFiltering.Filter(DecisionsData.Data());

        Assert.Equal(0, result.results.Count(r => r.matched.dolby_vision || r.matched.high_dynamic_range));
    }
    [Fact]
    public void Filter_Hdr_Disallowed_Dv_Allowed()
    {
        if (!Config.ARGS.dolby_vision_dv_allowed || Config.ARGS.high_dynamic_range_hdr_allowed)
            return;

        var data = DecisionsData.Data();
        var count = data.results.Count(r => r.matched.dolby_vision);
        var result = HighDynamicRangeDolbyVisionFiltering.Filter(data);

        Assert.Equal(count, result.results.Count(r => r.matched.dolby_vision || r.matched.high_dynamic_range));
    }
    [Fact]
    public void Filter_Hdr_Allowed_Dv_Disallowed()
    {
        if (Config.ARGS.dolby_vision_dv_allowed || !Config.ARGS.high_dynamic_range_hdr_allowed)
            return;

        var data = DecisionsData.Data();
        var count = data.results.Count(r => r.matched.high_dynamic_range);
        var result = HighDynamicRangeDolbyVisionFiltering.Filter(data);

        Assert.Equal(count, result.results.Count(r => r.matched.dolby_vision || r.matched.high_dynamic_range));
    }

    [Fact]
    public void Filter_Preserves_Request_And_SearchJob()
    {
        var data = DecisionsData.Data();
        var result = HighDynamicRangeDolbyVisionFiltering.Filter(data);
        Assert.Equal(data.request_job, result.request_job);
        Assert.Equal(data.search_job, result.search_job);
    }
}
