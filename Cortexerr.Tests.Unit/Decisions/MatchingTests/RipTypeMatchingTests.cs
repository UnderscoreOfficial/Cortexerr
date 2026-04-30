using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Tests.Unit.Decisions.MatchingTests;

public class RipTypeMatchingTests
{
    [Fact]
    public void Rip_Type_Match()
    {
        var names = DecisionsData.MatchBuilder(new[]
        {
            "remux",
            "blu ray",
            "bdmv",
            "bdrip",
            "web dl",
            "web rip",
            "hdtv",
            "sdtv",
            "pdtv",
            "dvb rip",
            "dvd rip",
            "dvd scr",
            "dvd",
            "scr",
            "screener",
            "cam",
            "camrip",
            "hd cam",
            "telesync",
            "telecine",
            "ts",
            "tc",
        });
        var valid_count = 0;
        var data = DecisionsData.Data();
        foreach (var name in names)
        {
            var result = RipTypeMatching.Match(data.request_job, data.search_job, name);
            if (result != null)
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
