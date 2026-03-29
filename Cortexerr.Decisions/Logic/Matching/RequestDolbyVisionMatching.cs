using System.Text.RegularExpressions;
using Cortexerr.Extended.DataStructures;
using Cortexerr.Extended.Indexer;

namespace Cortexerr.Decisions.Logic.Matching;

public static class RequestDolbyVisionMatching
{
    public static bool Match(RequestJob request_job, IndexerSearchJob search_job, string name)
    {
        return Regex.IsMatch(name, @"\b(dolby\.?vision|dv)\b");
    }
}
