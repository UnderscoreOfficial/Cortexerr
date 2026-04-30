using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Tests.Unit.Decisions.MatchingTests;

public class ResolutionMatchingTests
{
    [Fact]
    public void Resolution_Match()
    {
        var names = DecisionsData.MatchBuilder(new[]
        {
            "480p",
            "sd",
            "576p",
            "720p",
            "720i",
            "hd",
            "1080p",
            "1080i",
            "full hd",
            "2160p",
            "4k",
            "uhd",
            "ultra hd",
        });
        var valid_count = 0;
        var data = DecisionsData.Data();
        foreach (var name in names)
        {
            var result = ResolutionMatching.Match(data.request_job, data.search_job, name);
            if (result != null)
                valid_count++;
        }
        Assert.Equal(names.Count, valid_count);
    }
}
