using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Tests.Unit.Decisions.MatchingTests;

public class MatchingTests
{
    [Fact]
    public void Matching_Match()
    {
        var data = DecisionsData.Data();
        var result = Matching.Match(data.request_job, data.search_job);
        Assert.Equal(data.search_job.indexer_search_job.results.Count, result.results.Count);
    }
    [Fact]
    public void Matching_Preserves_Request_And_SearchJob()
    {
        var data = DecisionsData.Data();
        var result = Matching.Match(data.request_job, data.search_job);
        Assert.Equal(data.request_job, result.request_job);
        Assert.Equal(data.search_job, result.search_job);
    }
}
