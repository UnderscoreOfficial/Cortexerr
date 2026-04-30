using Cortexerr.Core.DataStructures;
using Cortexerr.Core.Ingest;
using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Decisions.Logic.Sorting;

public sealed record NormalizedSizes
{
    public required double x_size { get; init; }
    public required double y_size { get; init; }
    public required double x_size_multiplier { get; init; }
    public required double y_size_multiplier { get; init; }
}

public static class NormalizeSize
{
    public static NormalizedSizes Normalize(DecisionLogicRequestMatching x, DecisionLogicRequestMatching y, Ingest ingest)
    {
        // const double av1_size_multiplier = 1.8;
        // const double h265_size_multiplier = 1.4;
        const double av1_size_multiplier = 2.3;
        const double h265_size_multiplier = 2.0;

        var max_gb = 1000;
        var min_gb = .1;
        var power = 3;
        var x_size_multiplier = Math.Pow((Math.Log10(x.item.size) - Math.Log10(min_gb)) / (Math.Log10(max_gb) - Math.Log10(min_gb)), power) + 1;
        if (x_size_multiplier > 1.5)
            x_size_multiplier = 1.5;
        var y_size_multiplier = Math.Pow((Math.Log10(y.item.size) - Math.Log10(min_gb)) / (Math.Log10(max_gb) - Math.Log10(min_gb)), power) + 1;
        if (y_size_multiplier > 1.5)
            y_size_multiplier = 1.5;

        var x_size = x.matched.video_codec == VideoCodec.AV1 ? x.item.size * av1_size_multiplier
            : x.matched.video_codec == VideoCodec.H265 ? x.item.size * h265_size_multiplier
            : x.item.size;
        var y_size = y.matched.video_codec == VideoCodec.AV1 ? y.item.size * av1_size_multiplier
            : y.matched.video_codec == VideoCodec.H265 ? y.item.size * h265_size_multiplier
            : y.item.size;

        if (ingest.sonarr?.series.seasons != null)
        {
            foreach (var sonarr_season in ingest.sonarr.series.seasons)
            {
                var episode_count = sonarr_season.statistics?.total_episode_count;
                if (sonarr_season.season_number == ingest.sonarr.request.season && episode_count.HasValue)
                {
                    if (x.matched.series?.seasons.Length == 1 && x.matched.series.episodes.Length == 0)
                        x_size /= episode_count.Value;

                    if (y.matched.series?.seasons.Length == 1 && y.matched.series.episodes.Length == 0)
                        y_size /= episode_count.Value;
                }
            }
        }

        return new NormalizedSizes
        {
            x_size = x_size,
            y_size = y_size,
            x_size_multiplier = x_size_multiplier,
            y_size_multiplier = y_size_multiplier
        };
    }
}
