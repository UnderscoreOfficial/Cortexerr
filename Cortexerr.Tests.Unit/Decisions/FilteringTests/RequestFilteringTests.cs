using Cortexerr.Decisions.Logic.Filtering;

namespace Cortexerr.Tests.Unit.Decisions.FilteringTests;

public class RequestFilteringTests
{
    [Fact]
    public void Filter_Preserves_Request_And_SearchJob()
    {
        var data = DecisionsData.Data();
        var result = RequestFiltering.Filter(data);
        Assert.Equal(data.request_job, result.request_job);
        Assert.Equal(data.search_job, result.search_job);
    }
}
