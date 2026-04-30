using Cortexerr.Decisions.Logic.Matching;
using Cortexerr.Extended.Indexer;

namespace Cortexerr.Decisions.Logic.Sorting;

public static class DateSorting
{
    public static int Sort(DecisionLogicRequestMatching x, DecisionLogicRequestMatching y)
    {
        if (x.item.type == IndexerResultType.NZBHYDRA && x.item.type == IndexerResultType.NZBHYDRA)
        {
            if (x.item.upload_date.HasValue && y.item.upload_date.HasValue)
            {
                if (x.item.upload_date.Value.Year == y.item.upload_date.Value.Year)
                {
                    return 0;
                }
                else if (x.item.upload_date.Value.Year > y.item.upload_date.Value.Year)
                {
                    return -1;
                }
                else if (y.item.upload_date.Value.Year > x.item.upload_date.Value.Year)
                {
                    return 1;
                }
            }
        }
        return 0;
    }
}
