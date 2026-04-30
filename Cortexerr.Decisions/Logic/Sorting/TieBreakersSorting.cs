using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Decisions.Logic.Sorting;

public static class TieBreakersSorting
{
    public static int Sort(DecisionLogicRequestMatching x, DecisionLogicRequestMatching y)
    {
        var audio_codec = AudioCodecSorting.Sort(x, y);
        var resolution = ResolutionSorting.Sort(x, y);
        var high_dynamic_range = HighDynamicRangeSorting.Sort(x, y);
        var dolby_vision = DolbyVisionSorting.Sort(x, y);
        var release_group = ReleaseGroupSorting.Sort(x, y);
        var riptype = RipTypeSorting.Sort(x, y);
        var date = DateSorting.Sort(x, y);

        if (riptype != 0)
            return riptype;
        if (date != 0)
            return date;
        if (release_group != 0)
            return release_group;
        if (high_dynamic_range != 0)
            return high_dynamic_range;
        if (dolby_vision != 0)
            return dolby_vision;
        if (audio_codec != 0)
            return audio_codec;
        if (resolution != 0)
            return resolution;
        // fallback to size if no tiebreakers work
        if (x.item.size > y.item.size)
            return -1;
        if (y.item.size > x.item.size)
            return 1;
        return 0;
    }
}
