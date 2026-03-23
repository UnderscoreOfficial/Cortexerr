using System.Text.Json;
using System.Text.Json.Serialization;
using Cortexerr.Core.Configuration;
using Cortexerr.Core.Errors;
using Cortexerr.Core.Logging;

namespace Cortexerr.Core.Arrs;

public sealed record RadarrResponseMediaInfo
{
    public int? audio_bitrate { get; init; }
    public double? audio_channels { get; init; }
    public string? audio_codec { get; init; }
    public string? audio_languages { get; init; }
    public int? audio_stream_count { get; init; }
    public int? video_bit_depth { get; init; }
    public int? video_bitrate { get; init; }
    public string? video_codec { get; init; }
    public double? video_fps { get; init; }
    public string? video_dynamic_range { get; init; }
    public string? video_dynamic_range_type { get; init; }
    public string? resolution { get; init; }
    public string? run_time { get; init; }
    public string? scan_type { get; init; }
    public string? subtitles { get; init; }
}

public sealed record RadarrResponseStatistics
{
    public int? movie_file_count { get; init; }
    public long? size_on_disk { get; init; }
    public string[]? release_groups { get; init; }
}

public sealed record RadarrResponseRevision
{
    public int? version { get; init; }
    public int? real { get; init; }
    [JsonPropertyName("isRepack")]
    public bool? is_repack { get; init; }
}
public sealed record RadarrResponseQualityDetail
{
    public int? id { get; init; }
    public string? name { get; init; }
    public string? source { get; init; }
    public int? resolution { get; init; }
    public string? modifier { get; init; }
}
public sealed record RadarrResponseQuality
{
    public RadarrResponseQualityDetail? quality { get; init; }
    public RadarrResponseRevision? revision { get; init; }
}

public sealed record RadarrResponseMovieFile
{
    public int? movie_id { get; init; }
    public string? relative_path { get; init; }
    public string? path { get; init; }
    public long? size { get; init; }
    public string? date_added { get; init; }
    public string? release_group { get; init; }
    public string? edition { get; init; }
    public RadarrResponseLanguage[]? languages { get; init; }
    public RadarrResponseQuality? quality { get; init; }
    public int? indexer_flags { get; init; }
    public RadarrResponseMediaInfo? media_info { get; init; }
    public bool? quality_cutoff_not_met { get; init; }
    public int? id { get; init; }
}

public sealed record RadarrResponseRatingDetail
{
    public int? votes { get; init; }
    public double? value { get; init; }
    public string? type { get; init; }
}

public sealed record RadarrResponseRatings
{
    public RadarrResponseRatingDetail? imdb { get; init; }
    public RadarrResponseRatingDetail? tmdb { get; init; }
    public RadarrResponseRatingDetail? metacritic { get; init; }
    [JsonPropertyName("rottenTomatoes")]
    public RadarrResponseRatingDetail? rotten_tomatoes { get; init; }
    public RadarrResponseRatingDetail? trakt { get; init; }
}

public sealed record RadarrResponseImage
{
    public string? cover_type { get; init; }
    public string? url { get; init; }
    public string? remote_url { get; init; }
}

public sealed record RadarrResponseAlternateTitle
{
    public string? source_type { get; init; }
    public int? movie_metadata_id { get; init; }
    public string? title { get; init; }
    public int? id { get; init; }
}

public sealed record RadarrResponseLanguage
{
    public int? id { get; init; }
    public string? name { get; init; }
}

public sealed record RadarrResponseMovie
{
    public string? title { get; init; }
    [JsonPropertyName("originalTitle")]
    public string? original_title { get; init; }
    [JsonPropertyName("originalLanguage")]
    public RadarrResponseLanguage? original_language { get; init; }
    [JsonPropertyName("alternateTitles")]
    public RadarrResponseAlternateTitle[]? alternate_titles { get; init; }
    [JsonPropertyName("secondaryYearSourceId")]
    public int? secondary_year_source_id { get; init; }
    [JsonPropertyName("sortTitle")]
    public string? sort_title { get; init; }
    [JsonPropertyName("sizeOnDisk")]
    public long? size_on_disk { get; init; }
    public string? status { get; init; }
    public string? overview { get; init; }
    [JsonPropertyName("inCinemas")]
    public string? in_cinemas { get; init; }
    [JsonPropertyName("physicalRelease")]
    public string? physical_release { get; init; }
    [JsonPropertyName("digitalRelease")]
    public string? digital_release { get; init; }
    [JsonPropertyName("releaseDate")]
    public string? release_date { get; init; }
    public RadarrResponseImage[]? images { get; init; }
    public string? website { get; init; }
    public int? year { get; init; }
    [JsonPropertyName("youTubeTrailerId")]
    public string? you_tube_trailer_id { get; init; }
    public string? studio { get; init; }
    public string? path { get; init; }
    [JsonPropertyName("qualityProfileId")]
    public int? quality_profile_id { get; init; }
    [JsonPropertyName("hasFile")]
    public bool? has_file { get; init; }
    [JsonPropertyName("movieFileId")]
    public int? movie_file_id { get; init; }
    public bool? monitored { get; init; }
    [JsonPropertyName("minimumAvailability")]
    public string? minimum_availability { get; init; }
    [JsonPropertyName("isAvailable")]
    public bool? is_available { get; init; }
    [JsonPropertyName("folderName")]
    public string? folder_name { get; init; }
    public int? runtime { get; init; }
    [JsonPropertyName("cleanTitle")]
    public string? clean_title { get; init; }
    [JsonPropertyName("imdbId")]
    public string? imdb_id { get; init; }
    [JsonPropertyName("tmdbId")]
    public int? tmdb_id { get; init; }
    [JsonPropertyName("titleSlug")]
    public string? title_slug { get; init; }
    [JsonPropertyName("rootFolderPath")]
    public string? root_folder_path { get; init; }
    public string? certification { get; init; }
    public string[]? genres { get; init; }
    public string[]? keywords { get; init; }
    public string[]? tags { get; init; }
    public string? added { get; init; }
    public RadarrResponseRatings? ratings { get; init; }
    [JsonPropertyName("movieFile")]
    public RadarrResponseMovieFile? movie_file { get; init; }
    public double? popularity { get; init; }
    [JsonPropertyName("lastSearchTime")]
    public string? last_search_time { get; init; }
    public RadarrResponseStatistics? statistics { get; init; }
    public int? id { get; init; }
}


public static class Radarr
{
    private static readonly HttpClient _client = CreateClient();

    private static HttpClient CreateClient()
    {
        var client = new HttpClient { BaseAddress = Config.ARGS.radarr };
        client.DefaultRequestHeaders.Add("X-Api-Key", Env.RADARR_API_KEY);
        return client;
    }

    async private static Task<HandleResponse<T>> Request<T>(string endpoint)
    {
        Logger.Log.Debug($"(Radarr|Request) url: ({Config.ARGS.radarr}{endpoint})");
        var handled_response = await Error.HandleAsync<T>(async () =>
        {
            var response = await _client.GetStringAsync(endpoint);
            var json = JsonSerializer.Deserialize<T>(response);
            if (json == null) throw new Exception();
            return json;
        });
        return handled_response;
    }

    async public static Task<HandleResponse<RadarrResponseMovie>> Movie(int tmdb_id)
    {
        var endpoint = $"api/v3/movie?tmdbId={tmdb_id}";
        var response = await Request<RadarrResponseMovie[]>(endpoint);
        if (response.error != null) return Response.Error<RadarrResponseMovie>(response.error.code, response.error.message);
        if (response.data?[0] != null)
        {
            return Response.Success(response.data[0]);
        }
        return Response.Error<RadarrResponseMovie>(ErrorCode.NOT_FOUND, "(Radarr|Movie) No results");
    }
}
