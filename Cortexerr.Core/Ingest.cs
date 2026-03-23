using System.Text.Json;
using System.Text.Json.Serialization;
using Cortexerr.Core.Arrs;

namespace Cortexerr.Core.Ingest;

public enum TorrentState
{
    ERROR,
    MISSING_FILES,
    UPLOADING,
    PAUSED_UP,
    QUEUED_UP,
    STALLED_UP,
    CHECKING_UP,
    FORCED_UP,
    ALLOCATING,
    DOWNLOADING,
    META_DL,
    PAUSED_DL,
    QUEUED_DL,
    STALLED_DL,
    CHECKING_DL,
    FORCED_DL,
    CHECKING_RESUME_DATA,
    MOVING,
    UNKNOWN
}
public static class TorrentStateParser
{
    public static TorrentState Parse(string value) => value switch
    {
        "error" => TorrentState.ERROR,
        "missingFiles" => TorrentState.MISSING_FILES,
        "uploading" => TorrentState.UPLOADING,
        "pausedUP" => TorrentState.PAUSED_UP,
        "queuedUP" => TorrentState.QUEUED_UP,
        "stalledUP" => TorrentState.STALLED_UP,
        "checkingUP" => TorrentState.CHECKING_UP,
        "forcedUP" => TorrentState.FORCED_UP,
        "allocating" => TorrentState.ALLOCATING,
        "downloading" => TorrentState.DOWNLOADING,
        "metaDL" => TorrentState.META_DL,
        "pausedDL" => TorrentState.PAUSED_DL,
        "queuedDL" => TorrentState.QUEUED_DL,
        "stalledDL" => TorrentState.STALLED_DL,
        "checkingDL" => TorrentState.CHECKING_DL,
        "forcedDL" => TorrentState.FORCED_DL,
        "checkingResumeData" => TorrentState.CHECKING_RESUME_DATA,
        "moving" => TorrentState.MOVING,
        _ => TorrentState.UNKNOWN
    };
}
public sealed class TorrentStateConverter : JsonConverter<TorrentState>
{
    public override TorrentState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => TorrentStateParser.Parse(reader.GetString() ?? "unknown");
    public override void Write(Utf8JsonWriter writer, TorrentState value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}

public interface IIngestConsumer
{
    void RequestHandler(Ingest ingest);
}

public sealed record IngestSonarrRequest
{
    public required int rid { get; init; }
    public required int tvdb_id { get; init; }
    public required double length { get; init; }
    public required string release { get; init; }
    public int? season { get; init; }
    public int? episode { get; init; }
}
public sealed record IngestSonarr
{
    public required SonarrResponseSeries series { get; init; }
    public required IngestSonarrRequest request { get; init; }
}

public sealed record IngestRadarrRequest
{
    public required int rid { get; init; }
    public required int tmdb_id { get; init; }
    public required double length { get; init; }
}
public sealed record IngestRadarr
{
    public required RadarrResponseMovie movie { get; init; }
    public required IngestRadarrRequest request { get; init; }
}

public sealed record IngestStatus
{
    public required string name { get; set; }
    public double size { get; set; }
    public float progress { get; set; }
    public int download_speed { get; set; }
    [JsonConverter(typeof(TorrentStateConverter))]
    public TorrentState state { get; set; }
    public required string save_path { get; set; }
    public string? content_path { get; set; }
    public int eta { get; set; }
    public bool completed { get; set; }
}

public sealed record Ingest
{
    public required string hash { get; init; }
    public IngestSonarr? sonarr { get; init; }
    public IngestRadarr? radarr { get; init; }
    public required IngestStatus status { get; init; }
}

