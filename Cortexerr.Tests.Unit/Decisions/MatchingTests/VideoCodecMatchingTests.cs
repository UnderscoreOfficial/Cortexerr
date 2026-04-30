using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Tests.Unit.Decisions.MatchingTests;

public class VideoCodecMatchingTests
{
    [Fact]
    public void Video_Codec_Match()
    {
        var names = DecisionsData.MatchBuilder(new[]
        {
            "x265",
            "h 265",
            "hevc",
            "x264",
            "h 264",
            "avc",
            "av1",
            "xvid",
            "divx",
            "mpeg 2",
            "mpeg 4",
            "vc 1",
        });
        var valid_count = 0;
        var data = DecisionsData.Data();
        foreach (var name in names)
        {
            var result = VideoCodecMatching.Match(data.request_job, data.search_job, name);
            if (result != null)
                valid_count++;
        }
        Assert.Equal(names.Count, valid_count);
    }
}
