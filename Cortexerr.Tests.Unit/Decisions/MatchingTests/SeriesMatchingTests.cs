using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Tests.Unit.Decisions.MatchingTests;

public class SeriesMatchingTests
{
    [Fact]
    public void Series_Match()
    {
        var names = DecisionsData.MatchBuilder(new[]
        {
            "complete",
            "full series",
            "the complete series",
            "complete season",
            "s01 e01",
            "s01",
            "e01",
            "season 1",
            "episode 1",
            "1x1",
            "ep 1",
        });
        var valid_count = 0;
        var data = DecisionsData.Data();
        foreach (var name in names)
        {
            var result = SeriesMatching.Match(data.request_job, data.search_job, name);
            if (result.seasons.Length > 0 || result.episodes.Length > 0 || result.pack == PackType.FULL)
            {
                valid_count++;
            }
            else
            {
                Console.WriteLine(name);
            }
        }
        Assert.Equal(names.Count, valid_count);
    }
}
