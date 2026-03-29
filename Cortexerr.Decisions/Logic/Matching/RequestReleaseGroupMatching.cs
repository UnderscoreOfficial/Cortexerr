using System.Text.RegularExpressions;
using Cortexerr.Core.Configuration;
using Cortexerr.Extended.DataStructures;
using Cortexerr.Extended.Indexer;

namespace Cortexerr.Decisions.Logic.Matching;

public static class RequestReleaseGroupMatching
{
    public static string? Match(RequestJob request_job, IndexerSearchJob search_job, string name)
    {
        foreach (var release_group in Config.ARGS.release_groups)
        {
            var group = release_group.ToLowerInvariant().Trim();
            if (Regex.IsMatch(name, $@"\b{group}\b")) return release_group;
        }
        return null;
    }
}
