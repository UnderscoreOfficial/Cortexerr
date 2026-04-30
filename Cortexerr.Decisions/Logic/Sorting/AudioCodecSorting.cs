using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Decisions.Logic.Sorting;

public static class AudioCodecSorting
{
    public static int Sort(DecisionLogicRequestMatching x, DecisionLogicRequestMatching y)
    {
        if (x.matched.audio_codec.HasValue && y.matched.audio_codec == null)
            return -1;
        if (x.matched.audio_codec == null && y.matched.audio_codec.HasValue)
            return 1;
        if (x.matched.audio_codec.HasValue && y.matched.audio_codec.HasValue)
        {
            if (x.matched.audio_codec.Value > y.matched.audio_codec.Value)
                return -1;
            else if (x.matched.audio_codec.Value < y.matched.audio_codec.Value)
                return 1;
        }
        return 0;
    }
}
