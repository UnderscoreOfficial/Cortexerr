using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Tests.Unit.Decisions.MatchingTests;

public class DolbyVisionMatchingTests
{
    [Fact]
    public void Dolby_Vision_Match()
    {
        var names = DecisionsData.MatchBuilder(new[]
        {
            "dolby vision",
            "dv",
        });
        var valid_count = 0;
        var data = DecisionsData.Data();
        foreach (var name in names)
        {
            var result = DolbyVisionMatching.Match(data.request_job, data.search_job, name);
            if (result)
                valid_count++;
        }
        Assert.Equal(names.Count, valid_count);
    }
}
