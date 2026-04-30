using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Decisions.Logic.Sorting;

public static class SeriesSorting
{
    public static bool Sort(DecisionLogicRequestMatching x, DecisionLogicRequestMatching y, NormalizedSeries normalized_series)
    {
        if (x.matched.series == null || y.matched.series == null)
            return false;
        var x_series = x.matched.series;
        var y_series = y.matched.series;

        var x_pass = false;
        var y_pass = false;


        // tmp limited to only single seasons and full seasons eventually I will expand it out to figure out 
        // how multi ep / complete series fit into this
        //
        // logic will be moved to filter
        //
        // basiclaly using the data as a reference sample and using median/averages to ideally remove 
        // potentially mislabled or matched season packs as single episodes or the inverse 
        // if an item marked as a single ep is showing a size greter than the median season pack sizes
        // its very likely this is not a single ep

        if (x_series.seasons.Length == 1 && x_series.episodes.Length == 1)
        {
            if (x.item.size <= normalized_series.season_packs_median)
                x_pass = true;
        }
        if (x_series.seasons.Length == 1 && x_series.episodes.Length == 0)
        {
            // this is less needed but has a higher false positive as 
            // some very compressed season packs can be comparable to a median but more or less
            // this will likely have to change
            if (x.item.size >= normalized_series.single_episodes_median)
                x_pass = true;
        }
        if (y_series.seasons.Length == 1 && y_series.episodes.Length == 1)
        {
            if (y.item.size <= normalized_series.season_packs_median)
                y_pass = true;
        }
        if (y_series.seasons.Length == 1 && y_series.episodes.Length == 0)
        {
            if (y.item.size >= normalized_series.single_episodes_median)
                y_pass = true;
        }
        if (x_pass == true && y_pass == true)
        {
            return true;
        }
        return false;
    }
}
