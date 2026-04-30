using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Decisions.Logic.Sorting;

public static class DolbyVisionSorting
{
    public static int Sort(DecisionLogicRequestMatching x, DecisionLogicRequestMatching y)
    {
        if (x.matched.dolby_vision && !y.matched.dolby_vision)
            return -1;
        else if (!x.matched.dolby_vision && y.matched.dolby_vision)
            return 1;
        return 0;
    }
}
