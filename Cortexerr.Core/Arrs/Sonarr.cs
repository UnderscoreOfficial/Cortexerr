using System.Text.Json;
using System.Text.Json.Serialization;
using Cortexerr.Core.Configuration;
using Cortexerr.Core.Errors;
using Cortexerr.Core.Logging;

namespace Cortexerr.Core.Arrs;

public sealed record SonarrResponseStatistics
{
    [JsonPropertyName("seasonCount")]
    public int? season_count { get; init; }
    [JsonPropertyName("episodeFileCount")]
    public int? episode_file_count { get; init; }
    [JsonPropertyName("episodeCount")]
    public int? episode_count { get; init; }
    [JsonPropertyName("totalEpisodeCount")]
    public int? total_episode_count { get; init; }
    [JsonPropertyName("sizeOnDisk")]
    public long? size_on_disk { get; init; }
    [JsonPropertyName("releaseGroups")]
    public string[]? release_groups { get; init; }
    [JsonPropertyName("percentOfEpisodes")]
    public double? percent_of_episodes { get; init; }
}

public sealed record SonarrResponseSeasonStatistics
{
    [JsonPropertyName("previousAiring")]
    public string? previous_airing { get; init; }
    [JsonPropertyName("episodeFileCount")]
    public int? episode_file_count { get; init; }
    [JsonPropertyName("episodeCount")]
    public int? episode_count { get; init; }
    [JsonPropertyName("totalEpisodeCount")]
    public int? total_episode_count { get; init; }
    [JsonPropertyName("sizeOnDisk")]
    public long? size_on_disk { get; init; }
    [JsonPropertyName("releaseGroups")]
    public string[]? release_groups { get; init; }
    [JsonPropertyName("percentOfEpisodes")]
    public double? percent_of_episodes { get; init; }
}

public sealed record SonarrResponseRatings
{
    public int? votes { get; init; }
    public double? value { get; init; }
}

public sealed record SonarrResponseSeason
{
    [JsonPropertyName("seasonNumber")]
    public int? season_number { get; init; }
    public bool? monitored { get; init; }
    public SonarrResponseSeasonStatistics? statistics { get; init; }
}

public sealed record SonarrResponseLanguage
{
    public int? id { get; init; }
    public string? name { get; init; }
}

public sealed record SonarrResponseAlternateTitle
{
    public string? title { get; init; }
    [JsonPropertyName("seasonNumber")]
    public int? season_number { get; init; }
    [JsonPropertyName("sceneSeasonNumber")]
    public int? scene_season_number { get; init; }
    [JsonPropertyName("sceneOrigin")]
    public string? scene_origin { get; init; }
    public string? comment { get; init; }
}

public sealed record SonarrResponseImage
{
    [JsonPropertyName("coverType")]
    public string? cover_type { get; init; }
    public string? url { get; init; }
    [JsonPropertyName("remoteUrl")]
    public string? remote_url { get; init; }
}

public sealed record SonarrResponseSeries
{
    public string? title { get; init; }
    [JsonPropertyName("alternateTitles")]
    public SonarrResponseAlternateTitle[]? alternate_titles { get; init; }
    [JsonPropertyName("sortTitle")]
    public string? sort_title { get; init; }
    public string? status { get; init; }
    public bool? ended { get; init; }
    public string? overview { get; init; }
    [JsonPropertyName("previousAiring")]
    public string? previous_airing { get; init; }
    public string? network { get; init; }
    [JsonPropertyName("airTime")]
    public string? air_time { get; init; }
    public SonarrResponseImage[]? images { get; init; }
    [JsonPropertyName("originalLanguage")]
    public SonarrResponseLanguage? original_language { get; init; }
    public SonarrResponseSeason[]? seasons { get; init; }
    public int? year { get; init; }
    public string? path { get; init; }
    [JsonPropertyName("qualityProfileId")]
    public int? quality_profile_id { get; init; }
    [JsonPropertyName("seasonFolder")]
    public bool? season_folder { get; init; }
    public bool? monitored { get; init; }
    [JsonPropertyName("monitorNewItems")]
    public string? monitor_new_items { get; init; }
    [JsonPropertyName("useSceneNumbering")]
    public bool? use_scene_numbering { get; init; }
    public int? runtime { get; init; }
    [JsonPropertyName("tvdbId")]
    public int? tvdb_id { get; init; }
    [JsonPropertyName("tvRageId")]
    public int? tv_rage_id { get; init; }
    [JsonPropertyName("tvMazeId")]
    public int? tv_maze_id { get; init; }
    [JsonPropertyName("tmdbId")]
    public int? tmdb_id { get; init; }
    [JsonPropertyName("firstAired")]
    public string? first_aired { get; init; }
    [JsonPropertyName("lastAired")]
    public string? last_aired { get; init; }
    [JsonPropertyName("seriesType")]
    public string? series_type { get; init; }
    [JsonPropertyName("cleanTitle")]
    public string? clean_title { get; init; }
    [JsonPropertyName("imdbId")]
    public string? imdb_id { get; init; }
    [JsonPropertyName("titleSlug")]
    public string? title_slug { get; init; }
    [JsonPropertyName("rootFolderPath")]
    public string? root_folder_path { get; init; }
    public string? certification { get; init; }
    public string[]? genres { get; init; }
    public int[]? tags { get; init; }
    public string? added { get; init; }
    public SonarrResponseRatings? ratings { get; init; }
    public SonarrResponseStatistics? statistics { get; init; }
    [JsonPropertyName("languageProfileId")]
    public int? language_profile_id { get; init; }
    public int? id { get; init; }
}

public static class Sonarr
{
    private static readonly HttpClient _client = CreateClient();

    private static HttpClient CreateClient()
    {
        var client = new HttpClient { BaseAddress = Config.ARGS.sonarr };
        client.DefaultRequestHeaders.Add("X-Api-Key", Env.SONARR_API_KEY);
        return client;
    }

    async private static Task<HandleResponse<T>> Request<T>(string endpoint)
    {
        Logger.Log.Debug($"(Sonarr|Request) url: ({Config.ARGS.sonarr}{endpoint})");
        var handled_response = await Error.HandleAsync<T>(async () =>
        {
            var response = await _client.GetStringAsync(endpoint);
            var json = JsonSerializer.Deserialize<T>(response);
            if (json == null) throw new Exception();
            return json;
        });
        return handled_response;
    }

    async public static Task<HandleResponse<SonarrResponseSeries>> Series(int tvdb_id)
    {
        var endpoint = $"api/v3/series?tvdbId={tvdb_id}";
        var response = await Request<SonarrResponseSeries[]>(endpoint);
        if (response.error != null) return Response.Error<SonarrResponseSeries>(response.error.code, response.error.message);
        if (response.data?[0] != null)
        {
            return Response.Success(response.data[0]);
        }
        return Response.Error<SonarrResponseSeries>(ErrorCode.NOT_FOUND, "(Sonarr|Series) No results");
    }
}
