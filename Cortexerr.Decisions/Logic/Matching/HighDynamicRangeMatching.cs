using System.Text.RegularExpressions;
using Cortexerr.Extended.DataStructures;
using Cortexerr.Extended.Indexer;

namespace Cortexerr.Decisions.Logic.Matching;

public static class HighDynamicRangeMatching
{
    public static bool Match(RequestJob request_job, IndexerSearchJob search_job, string name)
    {
        return Regex.IsMatch(name, @"((?<![^\s])hdr10\+(?![^\s])|\b(hdr10plus|hdr10|hdr|hlg)\b)");
    }
}
