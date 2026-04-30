using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Decisions.Logic.Sorting;

public static class HighDynamicRangeSorting
{
    public static int Sort(DecisionLogicRequestMatching x, DecisionLogicRequestMatching y)
    {
        if (x.matched.high_dynamic_range && !y.matched.high_dynamic_range)
            return -1;
        else if (!x.matched.high_dynamic_range && y.matched.high_dynamic_range)
            return 1;
        return 0;
    }
}
