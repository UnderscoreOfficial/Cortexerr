using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Decisions.Logic.Sorting;

public static class RipTypeSorting
{
    public static int Sort(DecisionLogicRequestMatching x, DecisionLogicRequestMatching y)
    {
        if (x.matched.riptype.HasValue && y.matched.riptype == null)
            return -1;
        if (x.matched.riptype == null && y.matched.riptype.HasValue)
            return 1;
        if (x.matched.riptype.HasValue && y.matched.riptype.HasValue)
        {
            if (x.matched.riptype.Value > y.matched.riptype.Value)
                return -1;
            else if (x.matched.riptype.Value < y.matched.riptype.Value)
                return 1;
        }
        return 0;
    }
}
