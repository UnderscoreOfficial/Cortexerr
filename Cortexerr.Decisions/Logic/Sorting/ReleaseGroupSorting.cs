using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Decisions.Logic.Sorting;

public static class ReleaseGroupSorting
{
    public static int Sort(DecisionLogicRequestMatching x, DecisionLogicRequestMatching y)
    {
        if (!string.IsNullOrEmpty(x.matched.release_group) && string.IsNullOrEmpty(y.matched.release_group))
            return -1;
        if (string.IsNullOrEmpty(x.matched.release_group) && !string.IsNullOrEmpty(y.matched.release_group))
            return 1;
        return 0;
    }
}
