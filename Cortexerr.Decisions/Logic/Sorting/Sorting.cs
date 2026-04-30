using Cortexerr.Core.DataStructures;
using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Decisions.Logic.Sorting;

public class Sorting(DecisionLogicMatchingJob match_job) : IComparer<DecisionLogicRequestMatching>
{
    public NormalizedSeries? normalized_series { get; } = NormalizeSeries.Normalize(match_job);
    public int Compare(DecisionLogicRequestMatching x, DecisionLogicRequestMatching y)
    {
        // 0 tie
        // -1 x wins
        // 1 y wins
        // if (match_job.request_job.ingest.sonarr != null && normalized_series != null)
        // {
        //     var series_sorting = SeriesSorting.Sort(x, y, normalized_series);
        //     if (!series_sorting)
        //         Console.WriteLine("Failed series sort invalid!");
        //     return 0;
        // }
        // var date = DateSorting.Sort(x, y);
        // if (date == -1 || date == 1)
        //     return date;
        var normalized_sizes = NormalizeSize.Normalize(x, y, match_job.request_job.ingest);
        var size_sort = SizeSorting.Sort(normalized_sizes);

        if (x.matched.riptype == RipType.REMUX && y.matched.riptype == RipType.REMUX)
        {
            if (size_sort == 0)
                return TieBreakersSorting.Sort(x, y);
            return size_sort;
        }
        if (x.matched.riptype == RipType.REMUX)
            return -1;
        if (y.matched.riptype == RipType.REMUX)
            return 1;

        if (size_sort == 0)
            return TieBreakersSorting.Sort(x, y);
        return size_sort;
    }
}
