using Cortexerr.Core.Configuration;
using Cortexerr.Decisions.Logic.Filtering;

namespace Cortexerr.Tests.Unit.Decisions.FilteringTests;

public class RipTypeFilteringTests
{
    [Fact]
    public void RipType_Filter()
    {
        var data = DecisionsData.Data();
        var count = data.results.Count(r => r.matched.riptype >= Config.ARGS.minimum_riptype);
        var result = RipTypeFiltering.Filter(data);
        Assert.Equal(count, result.results.Count(r => r.matched.riptype != null));
    }
    [Fact]
    public void Filter_Preserves_Request_And_SearchJob()
    {
        var data = DecisionsData.Data();
        var result = RipTypeFiltering.Filter(data);
        Assert.Equal(data.request_job, result.request_job);
        Assert.Equal(data.search_job, result.search_job);
    }
}
