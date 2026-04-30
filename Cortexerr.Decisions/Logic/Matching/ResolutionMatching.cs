using System.Text.RegularExpressions;
using Cortexerr.Core.DataStructures;
using Cortexerr.Extended.DataStructures;
using Cortexerr.Extended.Indexer;

namespace Cortexerr.Decisions.Logic.Matching;

public partial class ResolutionMatching
{
    [GeneratedRegex(@"\b(480p|sd)\b")]
    private static partial Regex R480pRegex();
    [GeneratedRegex(@"\b576p\b")]
    private static partial Regex R576pRegex();
    [GeneratedRegex(@"\b(720p|720i|hd)\b")]
    private static partial Regex R720pRegex();
    [GeneratedRegex(@"\b(1080p|1080i|fhd|full[\ \:\-]?hd)\b")]
    private static partial Regex R1080pRegex();
    [GeneratedRegex(@"\b(2160p|4k|uhd|ultra[\ \:\-]?hd)\b")]
    private static partial Regex R2160pRegex();

    public static Resolution? Match(RequestJob request_job, IndexerSearchJob search_job, string name)
    {
        if (R480pRegex().IsMatch(name)) return Resolution.R480p;
        if (R576pRegex().IsMatch(name)) return Resolution.R576p;
        if (R720pRegex().IsMatch(name)) return Resolution.R720p;
        if (R1080pRegex().IsMatch(name)) return Resolution.R1080p;
        if (R2160pRegex().IsMatch(name)) return Resolution.R2160p;
        return null;
    }
}
