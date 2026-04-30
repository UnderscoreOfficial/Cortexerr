using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Decisions.Logic.Sorting;

public static class ResolutionSorting
{
    public static int Sort(DecisionLogicRequestMatching x, DecisionLogicRequestMatching y)
    {
        if (x.matched.resolution.HasValue && y.matched.resolution == null)
            return -1;
        if (x.matched.resolution == null && y.matched.resolution.HasValue)
            return 1;
        if (x.matched.resolution.HasValue && y.matched.resolution.HasValue)
        {
            if (x.matched.resolution.Value > y.matched.resolution.Value)
                return -1;
            else if (x.matched.resolution.Value < y.matched.resolution.Value)
                return 1;
        }
        return 0;
    }
}
