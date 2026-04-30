using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Decisions.Logic.Sorting;

public static class VideoCodecSorting
{
    // unused
    public static int Sort(DecisionLogicRequestMatching x, DecisionLogicRequestMatching y)
    {
        if (x.matched.video_codec.HasValue && y.matched.video_codec == null)
            return -1;
        if (x.matched.video_codec == null && y.matched.video_codec.HasValue)
            return 1;
        if (x.matched.video_codec.HasValue && y.matched.video_codec.HasValue)
        {
            if (x.matched.video_codec.Value > y.matched.video_codec.Value)
                return -1;
            else if (x.matched.video_codec.Value < y.matched.video_codec.Value)
                return 1;
        }
        return 0;
    }
}
