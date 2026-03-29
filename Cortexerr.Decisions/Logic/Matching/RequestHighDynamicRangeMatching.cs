using System.Text.RegularExpressions;
using Cortexerr.Extended.DataStructures;
using Cortexerr.Extended.Indexer;

namespace Cortexerr.Decisions.Logic.Matching;

public static class RequestHighDynamicRangeMatching
{
    public static bool Match(RequestJob request_job, IndexerSearchJob search_job, string name)
    {
        return Regex.IsMatch(name, @"\b(hdr10\+|hdr10plus|hdr10|hdr|hlg)\b");
    }
}
