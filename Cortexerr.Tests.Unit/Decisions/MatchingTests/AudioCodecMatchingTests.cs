using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Tests.Unit.Decisions.MatchingTests;

public class AudioCodecMatchingTests
{
    [Fact]
    public void Audio_Codec_Match()
    {
        var names = DecisionsData.MatchBuilder(new[]
        {
            "truehd atmos",
            "atmos",
            "truehd",
            "dts x",
            "dts hd",
            "dts",
            "eac 3",
            "e ac 3",
            "ddplus",
            "dolby digital plus",
            "dd+",
            "ac 3",
            "dolby digital",
            "aac",
        });
        var valid_count = 0;
        var data = DecisionsData.Data();
        foreach (var name in names)
        {
            var result = AudioCodecMatching.Match(data.request_job, data.search_job, name);
            if (result != null)
                valid_count++;
        }
        Assert.Equal(names.Count, valid_count);
    }
}
