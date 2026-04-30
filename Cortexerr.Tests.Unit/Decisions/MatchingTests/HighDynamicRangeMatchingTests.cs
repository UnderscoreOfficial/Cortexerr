using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Tests.Unit.Decisions.MatchingTests;

public class HighDynamicRangeMatchingTests
{
    [Fact]
    public void High_Dynamic_Range_Match()
    {
        var names = DecisionsData.MatchBuilder(new[]
        {
            "hdr10+",
            "hdr10plus",
            "hdr10",
            "hdr",
            "hlg",
        });
        var valid_count = 0;
        var data = DecisionsData.Data();
        foreach (var name in names)
        {
            var result = HighDynamicRangeMatching.Match(data.request_job, data.search_job, name);
            if (result)
                valid_count++;
        }
        Assert.Equal(names.Count, valid_count);
    }
}
