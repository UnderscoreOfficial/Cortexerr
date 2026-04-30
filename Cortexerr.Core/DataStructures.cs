namespace Cortexerr.Core.DataStructures;

public enum RipType
{
    CAM,
    SCREENER,
    DVD,
    SDTV,
    HDTV,
    WEB_RIP,
    BLURAY,
    WEB_DL,
    REMUX
}

public enum Resolution
{
    R480p,
    R576p,
    R720p,
    R1080p,
    R2160p
}

public enum VideoCodec
{
    MPEG2,
    DIVX,
    XVID,
    MPEG4,
    H264,
    VC1,
    H265,
    AV1
}

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
