using System.Text.RegularExpressions;
using Cortexerr.Extended.DataStructures;
using Cortexerr.Extended.Indexer;

namespace Cortexerr.Decisions.Logic.Matching;

public static class RequestLanguageMatching
{
    public static string[] Match(RequestJob request_job, IndexerSearchJob search_job, string name)
    {
        var language_match_regex =
            @"\b(german|french|spanish|italian|portuguese|dutch|russian|japanese|korean|chinese|hindi|arabic|turkish|polish|swedish|norwegian|danish|finnish|hebrew|czech|hungarian|romanian|greek|thai|vietnamese|multi|vostfr|ita|ger|fre|spa|por|dut|rus|jap|kor|chi)\b";
        var matches = Regex.Matches(name, language_match_regex);
        var languages = new List<string>();
        foreach (Match match in matches)
        {
            if (match.Groups != null)
            {
                foreach (Group group in match.Groups)
                {
                    if (group.Success && !string.IsNullOrEmpty(group.Value))
                    {
                        languages.Add(group.Value);
                    }
                }
            }
        }
        return languages.ToArray();
    }
}
