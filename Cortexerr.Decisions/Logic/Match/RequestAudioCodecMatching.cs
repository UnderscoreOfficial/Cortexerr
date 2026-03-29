using System.Text.RegularExpressions;
using Cortexerr.Extended.DataStructures;
using Cortexerr.Extended.Indexer;

namespace Cortexerr.Decisions.Logic.Matching;

public enum AudioCodec
{
    AAC,
    AC3,           // Dolby Digital
    EAC3,          // Dolby Digital Plus / DD+
    DTS,
    DTS_HD,         // DTS-HD MA
    DTS_X,          // DTS:X
    TRUE_HD,        // Dolby TrueHD
    TRUE_HD_ATMOS,   // TrueHD with Atmos
    ATMOS,         // Atmos without confirmed TrueHD context
}

public partial class RequestAudioCodecMatching
{
    [GeneratedRegex(@"\btruehd[\. \-]?atmos\b")]
    private static partial Regex TrueHdAtmosRegex();
    [GeneratedRegex(@"\batmos\b")]
    private static partial Regex AtmosRegex();
    [GeneratedRegex(@"\btruehd\b")]
    private static partial Regex TrueHdRegex();
    [GeneratedRegex(@"\bdts[\. \-]?x\b")]
    private static partial Regex DtsXRegex();
    [GeneratedRegex(@"\bdts[\. \-]?hd\b")]
    private static partial Regex DtsHdRegex();
    [GeneratedRegex(@"\bdts\b")]
    private static partial Regex DtsRegex();
    [GeneratedRegex(@"\b(eac[\. \-]?3|e[\. \-]ac[\. \-]?3|dd\+|ddplus|dolby[\. \-]?digital[\. \-]?plus)\b")]
    private static partial Regex Eac3Regex();
    [GeneratedRegex(@"\b(ac[\. \-]?3|dolby[\. \-]?digital|dd)\b")]
    private static partial Regex Ac3Regex();
    [GeneratedRegex(@"\b(aac)\b")]
    private static partial Regex AacRegex();

    public static AudioCodec? Match(RequestJob request_job, IndexerSearchJob search_job, string name)
    {
        if (TrueHdAtmosRegex().IsMatch(name)) return AudioCodec.TRUE_HD_ATMOS;
        if (AtmosRegex().IsMatch(name)) return AudioCodec.ATMOS;
        if (TrueHdRegex().IsMatch(name)) return AudioCodec.TRUE_HD;
        if (DtsXRegex().IsMatch(name)) return AudioCodec.DTS_X;
        if (DtsHdRegex().IsMatch(name)) return AudioCodec.DTS_HD;
        if (DtsRegex().IsMatch(name)) return AudioCodec.DTS;
        if (Eac3Regex().IsMatch(name)) return AudioCodec.EAC3;
        if (Ac3Regex().IsMatch(name)) return AudioCodec.AC3;
        if (AacRegex().IsMatch(name)) return AudioCodec.AAC;
        return null;
    }
}
