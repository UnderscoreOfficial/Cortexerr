using System.Text.RegularExpressions;
using Cortexerr.Core.DataStructures;
using Cortexerr.Extended.DataStructures;
using Cortexerr.Extended.Indexer;

namespace Cortexerr.Decisions.Logic.Matching;

public partial class RequestVideoCodecMatching
{
    [GeneratedRegex(@"\b(x265|h[\. \-]?265|hevc)\b")]
    private static partial Regex H265Regex();
    [GeneratedRegex(@"\b(x264|h[\. \-]?264|avc)\b")]
    private static partial Regex H264Regex();
    [GeneratedRegex(@"\bav1\b")]
    private static partial Regex Av1Regex();
    [GeneratedRegex(@"\bxvid\b")]
    private static partial Regex XvidRegex();
    [GeneratedRegex(@"\bdivx\b")]
    private static partial Regex DivxRegex();
    [GeneratedRegex(@"\b(mpeg[\. \-]?2|mpeg2)\b")]
    private static partial Regex Mpeg2Regex();
    [GeneratedRegex(@"\b(mpeg[\. \-]?4|mpeg4)\b")]
    private static partial Regex Mpeg4Regex();
    [GeneratedRegex(@"\bvc[\. \-]?1\b")]
    private static partial Regex Vc1Regex();

    public static VideoCodec? Match(RequestJob request_job, IndexerSearchJob search_job, string name)
    {
        if (H265Regex().IsMatch(name)) return VideoCodec.H265;
        if (H264Regex().IsMatch(name)) return VideoCodec.H264;
        if (Av1Regex().IsMatch(name)) return VideoCodec.AV1;
        if (XvidRegex().IsMatch(name)) return VideoCodec.XVID;
        if (DivxRegex().IsMatch(name)) return VideoCodec.DIVX;
        if (Mpeg2Regex().IsMatch(name)) return VideoCodec.MPEG2;
        if (Mpeg4Regex().IsMatch(name)) return VideoCodec.MPEG4;
        if (Vc1Regex().IsMatch(name)) return VideoCodec.VC1;
        return null;
    }
}
