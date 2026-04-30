using Cortexerr.Core.Configuration;
using Cortexerr.Decisions.Logic.Filtering;

namespace Cortexerr.Tests.Unit.Decisions.FilteringTests;

public class SizeFilteringTests
{
    [Fact]
    public void Size_Filter()
    {
        if (Config.ARGS.download_max_size == 0) return;

        var data = DecisionsData.Data();
        var count = data.results.Count(r => r.item.size <= Config.ARGS.download_max_size);
        var result = SizeFiltering.Filter(data);
        Assert.Equal(count, result.results.Count);
    }
    [Fact]
    public void Filter_Preserves_Request_And_SearchJob()
    {
        var data = DecisionsData.Data();
        var result = SizeFiltering.Filter(data);
        Assert.Equal(data.request_job, result.request_job);
        Assert.Equal(data.search_job, result.search_job);
    }
}
