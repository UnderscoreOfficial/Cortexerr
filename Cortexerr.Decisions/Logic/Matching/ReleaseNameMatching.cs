using System.Text.RegularExpressions;
using Cortexerr.Extended.DataStructures;
using Cortexerr.Extended.Indexer;

namespace Cortexerr.Decisions.Logic.Matching;

public sealed record ReleaseNameMatchingResults
{
    public int token_set_ratio { get; init; }
    public int token_set_ratio_with_year { get; init; }
    public int ratio { get; init; }
    public int ratio_with_year { get; init; }
}

public static class ReleaseNameMatching
{
    public static ReleaseNameMatchingResults Match(RequestJob request_job, IndexerSearchJob search_job, string name)
    {
        var fuzzy_input = Regex.Replace(name, @"[._()[\]]", " ");
        fuzzy_input = Regex.Replace(fuzzy_input, @"\s+", " ").Trim();

        var sonarr = request_job.ingest.sonarr;
        var radarr = request_job.ingest.radarr;

        string? sort_title = null;
        int? year = null;
        if (sonarr != null)
        {
            sort_title = sonarr.series.sort_title;
            year = sonarr.series.year;
        }
        else if (radarr != null)
        {
            sort_title = radarr.movie.sort_title;
            year = radarr.movie.year;
        }


        var fuzzy_token_output = FuzzySharp.Fuzz.TokenSetRatio(fuzzy_input, sort_title);
        var fuzzy_ratio_output = FuzzySharp.Fuzz.Ratio(fuzzy_input, sort_title);

        var name_with_year = $"{sort_title} {year}";
        var fuzzy_token_output_year = FuzzySharp.Fuzz.TokenSetRatio(fuzzy_input, name_with_year);
        var fuzzy_ratio_output_year = FuzzySharp.Fuzz.Ratio(fuzzy_input, name_with_year);

        return new ReleaseNameMatchingResults
        {
            token_set_ratio = fuzzy_token_output,
            token_set_ratio_with_year = fuzzy_token_output_year,
            ratio = fuzzy_token_output,
            ratio_with_year = fuzzy_ratio_output_year
        };
    }
}
