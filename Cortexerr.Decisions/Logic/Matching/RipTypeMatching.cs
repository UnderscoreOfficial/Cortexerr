using System.Text.RegularExpressions;
using Cortexerr.Core.DataStructures;
using Cortexerr.Extended.DataStructures;
using Cortexerr.Extended.Indexer;

namespace Cortexerr.Decisions.Logic.Matching;

public partial class RipTypeMatching
{
    [GeneratedRegex(@"\bremux\b")]
    private static partial Regex RemuxRegex();
    [GeneratedRegex(@"\b(blu[\ \:\-]?ray|bdmv|bdrip)\b")]
    private static partial Regex BlurayRegex();
    [GeneratedRegex(@"\b(web[\ \:\-]?dl)\b")]
    private static partial Regex WebDlRegex();
    [GeneratedRegex(@"\b(web[\ \:\-]?rip)\b")]
    private static partial Regex WebRipRegex();
    [GeneratedRegex(@"\bhdtv\b")]
    private static partial Regex HdtvRegex();
    [GeneratedRegex(@"\b(sdtv|pdtv|dvb[\ \:\-]?rip)\b")]
    private static partial Regex SdtvRegex();
    [GeneratedRegex(@"\b(dvd[\ \:\-]?rip|dvd[\ \:\-]?scr|dvd)\b")]
    private static partial Regex DvdRegex();
    [GeneratedRegex(@"\b(scr|screener)\b")]
    private static partial Regex ScreenerRegex();
    [GeneratedRegex(@"\b(cam|camrip|hd[\ \:\-]?cam|telesync|telecine)\b")]
    private static partial Regex CamRegex();
    [GeneratedRegex(@"(?<![a-z])(ts|tc)(?![a-z])")]
    private static partial Regex TsTcRegex();

    public static RipType? Match(RequestJob request_job, IndexerSearchJob search_job, string name)
    {
        if (RemuxRegex().IsMatch(name)) return RipType.REMUX;
        if (BlurayRegex().IsMatch(name)) return RipType.BLURAY;
        if (WebDlRegex().IsMatch(name)) return RipType.WEB_DL;
        if (WebRipRegex().IsMatch(name)) return RipType.WEB_RIP;
        if (HdtvRegex().IsMatch(name)) return RipType.HDTV;
        if (SdtvRegex().IsMatch(name)) return RipType.SDTV;
        if (DvdRegex().IsMatch(name)) return RipType.DVD;
        if (ScreenerRegex().IsMatch(name)) return RipType.SCREENER;
        if (CamRegex().IsMatch(name)) return RipType.CAM;
        if (TsTcRegex().IsMatch(name)) return RipType.CAM;
        return null;
    }
}
