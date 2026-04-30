using System.Text.Json;
using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Decisions.Logic.Sorting;

public sealed record NormalizedSeries
{
    public long full_packs_average { get; init; }
    public long full_packs_median { get; init; }
    public long season_packs_average { get; init; }
    public long season_packs_median { get; init; }
    public long episode_packs_average { get; init; }
    public long episode_packs_median { get; init; }
    public long single_episodes_average { get; init; }
    public long single_episodes_median { get; init; }
}

public static class NormalizeSeries
{
    // idea of this is to establish a baseline of ep, season packs, full packs 
    // to ideally get a more accurate gauge when sorting to act as a edge case
    // when size is very different from expected median
    public static NormalizedSeries? Normalize(DecisionLogicMatchingJob matching_job)
    {
        var sonarr = matching_job.request_job.ingest.sonarr;
        if (sonarr != null)
        {
            var full_packs = new List<long>();
            long full_packs_size = 0;
            var episode_packs = new List<long>();
            long episode_packs_size = 0;
            var season_packs = new List<long>();
            long season_packs_size = 0;
            var single_episodes = new List<long>();
            long single_episodes_size = 0;

            var episode = matching_job.search_job.target.episode;
            var season = matching_job.search_job.target.season;

            if (sonarr.series.seasons != null)
            {
                foreach (var sonarr_season in sonarr.series.seasons)
                {
                    if (sonarr_season.season_number == season)
                    {
                        var count = sonarr_season.statistics?.total_episode_count;
                        foreach (var job in matching_job.results)
                        {
                            var matched_episodes = job.matched.series?.episodes;
                            var matched_seasons = job.matched.series?.seasons;
                            var pack = job.matched.series?.pack;

                            if (matched_seasons?.Length == 1 && matched_episodes?.Length == 1)
                            {
                                single_episodes.Add(job.item.size);
                                single_episodes_size += job.item.size;
                            }
                            else if (matched_seasons?.Length == 1 && matched_episodes?.Length == 0)
                            {
                                season_packs.Add(job.item.size);
                                season_packs_size += job.item.size;
                            }
                            else if (matched_episodes?.Length > 1)
                            {
                                episode_packs.Add(job.item.size);
                                episode_packs_size += job.item.size;
                            }
                            else if (matched_seasons?.Length > 1 || pack == PackType.FULL)
                            {
                                full_packs.Add(job.item.size);
                                full_packs_size += job.item.size;
                            }
                        }
                        break;
                    }
                }
            }
            full_packs.Sort();
            season_packs.Sort();
            episode_packs.Sort();
            single_episodes.Sort();

            // Console.WriteLine("full_packs: " + full_packs.Count);
            // Console.WriteLine(JsonSerializer.Serialize(full_packs));
            // Console.WriteLine("season_packs: " + season_packs.Count);
            // Console.WriteLine(JsonSerializer.Serialize(season_packs));
            // Console.WriteLine("episode_packs: " + episode_packs.Count);
            // Console.WriteLine(JsonSerializer.Serialize(episode_packs));
            // Console.WriteLine("single_episodes: " + single_episodes.Count);
            // Console.WriteLine(JsonSerializer.Serialize(single_episodes));

            long full_packs_average = 0;
            long full_packs_median = 0;
            if (full_packs.Count > 0)
            {
                full_packs_average = full_packs_size / full_packs.Count;
                full_packs_median = full_packs[full_packs.Count / 2];
                Console.WriteLine("full_packs_average: " + full_packs_average);
                Console.WriteLine("full_packs_median: " + full_packs_median);
            }

            long season_packs_average = 0;
            long season_packs_median = 0;
            if (season_packs.Count > 0)
            {
                season_packs_average = season_packs_size / season_packs.Count;
                season_packs_median = season_packs[season_packs.Count / 2];
                Console.WriteLine("season_packs_average: " + season_packs_average);
                Console.WriteLine("season_packs_median: " + season_packs_median);
            }

            long episode_packs_average = 0;
            long episode_packs_median = 0;
            if (episode_packs.Count > 0)
            {
                episode_packs_average = episode_packs_size / episode_packs.Count;
                episode_packs_median = episode_packs[episode_packs.Count / 2];
                Console.WriteLine("episode_packs_average: " + episode_packs_average);
                Console.WriteLine("episode_packs_median: " + episode_packs_median);
            }

            long single_episodes_average = 0;
            long single_episodes_median = 0;
            if (single_episodes.Count > 0)
            {
                single_episodes_average = single_episodes_size / single_episodes.Count;
                single_episodes_median = single_episodes[single_episodes.Count / 2];
                Console.WriteLine("single_episodes_average: " + single_episodes_average);
                Console.WriteLine("single_episodes_median: " + single_episodes_median);
            }
            return new NormalizedSeries
            {
                full_packs_average = full_packs_average,
                full_packs_median = full_packs_median,
                season_packs_average = season_packs_average,
                season_packs_median = season_packs_median,
                episode_packs_average = episode_packs_average,
                episode_packs_median = episode_packs_median,
                single_episodes_average = single_episodes_average,
                single_episodes_median = single_episodes_median
            };
        }
        return null;
    }
}
