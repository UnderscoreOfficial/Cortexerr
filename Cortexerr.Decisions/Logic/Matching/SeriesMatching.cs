using System.Text.RegularExpressions;
using Cortexerr.Extended.DataStructures;
using Cortexerr.Extended.Indexer;

namespace Cortexerr.Decisions.Logic.Matching;

public enum PackType
{
    SEASON,
    EPISODE,
    FULL,
}

public sealed record SeriesMatchingResults
{
    public required int[] seasons { get; init; }
    public required int[] episodes { get; init; }
    public PackType? pack { get; set; }
}

public static class SeriesMatching
{
    public static SeriesMatchingResults Match(RequestJob request_job, IndexerSearchJob search_job, string name)
    {
        var pack_keyword_match = @"\b(complete|full[\ \:\-]?series|the[\ \:\-]?complete[\ \:\-]?series|complete[\ \:\-]?series|complete[\ \:\-]?season)\b";
        var pack_match = Regex.IsMatch(name, pack_keyword_match);

        var ep_season_regex = @"(?:s(\d{1,2})|season[\ \:\-]?(\d+))(?:e(\d{1,2}))?|(\d{1,2})x(\d{1,2})|episode[\ \:\-]?(\d+)|ep[\ \:\-]?(\d+)|e(\d{1,2})";
        var main_match = Regex.Matches(name, ep_season_regex);

        var seasons = new List<int>();
        var episodes = new List<int>();
        foreach (Match match in main_match)
        {
            var episode_match = match.Groups[3].Success ? match.Groups[3].Value
                          : match.Groups[5].Success ? match.Groups[5].Value
                          : match.Groups[6].Success ? match.Groups[6].Value
                          : match.Groups[7].Success ? match.Groups[7].Value
                          : match.Groups[8].Value;

            if (int.TryParse(episode_match, out int episode_number))
            {
                episodes.Add(episode_number);
            }
            var season_match = match.Groups[1].Success ? match.Groups[1].Value
                 : match.Groups[2].Success ? match.Groups[2].Value
                 : match.Groups[4].Value;
            if (int.TryParse(season_match, out int season_number))
            {
                seasons.Add(season_number);
            }
        }
        return new SeriesMatchingResults
        {
            seasons = seasons.ToArray(),
            episodes = episodes.ToArray(),
            pack = pack_match ? PackType.FULL : null
        };
    }
}
