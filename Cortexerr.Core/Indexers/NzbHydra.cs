using System.Collections.Specialized;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Cortexerr.Core.Configuration;
using Cortexerr.Core.Errors;
using Cortexerr.Core.Logging;

namespace Cortexerr.Core.Indexers;

public record NzbHydraResponseIndexerStatus
{
    public string? indexer { get; init; }
    public string? state { get; init; }
    public int? level { get; init; }
    [JsonPropertyName("disabledUntil")]
    public string? disabled_until { get; init; }
    [JsonPropertyName("lastError")]
    public string? last_error { get; init; }
    [JsonPropertyName("apiResetTime")]
    public string? api_reset_time { get; init; }
    [JsonPropertyName("downloadResetTime")]
    public string? download_reset_time { get; init; }
    [JsonPropertyName("apiHits")]
    public int? api_hits { get; init; }
    [JsonPropertyName("apiHitLimit")]
    public int? api_hit_limit { get; init; }
    [JsonPropertyName("downloadHits")]
    public int? download_hits { get; init; }
    [JsonPropertyName("downloadHitLimit")]
    public int? download_hit_limit { get; init; }
    [JsonPropertyName("vipExpirationDate")]
    public string? vip_expiration_date { get; init; }
}

public record NzbHydraResponseResponse
{
    public NzbHydraResponseAttributes? attributes { get; init; }
}

public record NzbHydraResponseAttributes
{
    public string? offset { get; init; }
    public string? total { get; init; }
}

public record NzbHydraResponseEnclosureAttributes
{
    public string? url { get; init; }
    public string? length { get; init; }
    public string? type { get; init; }
}

public record NzbHydraResponseEnclosure
{
    public NzbHydraResponseEnclosureAttributes? attributes { get; init; }
}

public record NzbHydraResponseAttrAttributes
{
    public string? name { get; init; }
    public string? value { get; init; }
}

public record NzbHydraResponseAttr
{
    [JsonPropertyName("attributes")]
    public NzbHydraResponseAttrAttributes? attributes { get; init; }
}

public record NzbHydraResponseItem
{
    public string? title { get; init; }
    public string? guid { get; init; }
    public string? link { get; init; }
    public string? comments { get; init; }
    [JsonPropertyName("pubDate")]
    public double? pub_date { get; init; }
    public string? category { get; init; }
    public string? description { get; init; }
    public NzbHydraResponseEnclosure? enclosure { get; init; }
    public NzbHydraResponseAttr[]? attr { get; init; }
    public string? id { get; init; }
}

public record NzbHydraResponseChannel
{
    public string? title { get; init; }
    public string? link { get; init; }
    [JsonPropertyName("webMaster")]
    public string? web_master { get; init; }
    public object? category { get; init; }
    public NzbHydraResponseResponse? response { get; init; }
    public NzbHydraResponseItem[]? item { get; init; }
    public string? generator { get; init; }
}

public record NzbHydraResponseSearchResult
{
    public NzbHydraResponseChannel? channel { get; init; }
}

public record NzbHydraSearchParams(string search_type, List<int> categories, string? query, int? tvdb_id, int? tmdb_id, int? season, int? episode)
{
    public NameValueCollection ToQuery()
    {
        var query_string = HttpUtility.ParseQueryString($"apikey={Env.HYDRA_API_KEY}&offset=0&limit=100&o=json");
        query_string.Add("t", search_type);
        if (query != null) query_string.Add("q", query);
        if (tvdb_id != null) query_string.Add("tvdbid", tvdb_id.ToString());
        if (tmdb_id != null) query_string.Add("tmdbid", tmdb_id.ToString());
        if (season.HasValue) query_string.Add("season", season.Value.ToString());
        if (episode.HasValue) query_string.Add("ep", episode.Value.ToString());
        foreach (var category in categories)
        {
            query_string.Add("cat", category.ToString());
        }
        return query_string;
    }
}

public record NzbHydraIndexerDetails
{
    public required Uri url { get; init; }
    public required NzbHydraResponseIndexerStatus indexer { get; init; }
}

public record NzbHydraIndexers
{
    public required List<NzbHydraIndexerDetails> indexers { get; init; }
    public required NzbHydraSearchParams search_params { get; init; }
}

public record NzbHyrdraIndexerSearchItem
{
    public required NzbHydraIndexerDetails indexer { get; init; }
    public required HandleResponse<NzbHydraResponseSearchResult> results { get; init; }
}

public record NzbHydraIndexerResults
{
    public required int count { get; init; }
}

public record NzbHydraIndexerDetailsResults(NzbHydraIndexerDetails indexer_details, HandleResponse<NzbHydraIndexerResults> indexer_results);

public record NzbHydraSearchResults
{
    public required List<NzbHydraResponseItem> results { get; init; }
    public required List<NzbHydraIndexerDetailsResults> indexers { get; init; }
}

public static class NzbHydra
{
    private static readonly HttpClient _client = new()
    {
        BaseAddress = Config.ARGS.hydra
    };

    async private static Task<HandleResponse<T>> RequestGet<T>(string endpoint)
    {
        Logger.Log.Debug($"(NzbHydra|Request) url: ({Config.ARGS.hydra}{endpoint})");
        var handled_response = await Error.HandleAsync<T>(async () =>
        {
            var response = await _client.GetStringAsync(endpoint);
            var json = JsonSerializer.Deserialize<T>(response);
            if (json == null) throw new Exception();
            return json;
        });
        return handled_response;
    }

    async private static Task<HandleResponse<T>> RequestPost<T>(string endpoint, HttpContent content)
    {
        Logger.Log.Debug($"(NzbHydra|Request) url: ({Config.ARGS.hydra}{endpoint})");
        var handled_response = await Error.HandleAsync<T>(async () =>
        {
            var response = await _client.PostAsync(endpoint, content);
            string json_string = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<T>(json_string);
            if (json == null) throw new Exception();
            return json;
        });
        return handled_response;
    }

    async private static Task<HandleResponse<NzbHydraIndexers>> Indexers(NzbHydraSearchParams search_params)
    {
        var content = new StringContent($"{{\"apikey\":\"{Env.HYDRA_API_KEY}\"}}", Encoding.UTF8, "application/json");
        var endpoint = "api/stats/indexers";
        var response = await RequestPost<NzbHydraResponseIndexerStatus[]>(endpoint, content);
        if (response.error != null)
            return Response.Error<NzbHydraIndexers>(response.error.code, response.error.message);
        if (response.data == null)
            return Response.Error<NzbHydraIndexers>(ErrorCode.NOT_FOUND, "(NzbHydra|Indexers) No indexers found");

        var indexer_details = new List<NzbHydraIndexerDetails>();
        foreach (var indexer in response.data)
        {
            if (indexer.indexer != null && indexer.state == "ENABLED")
            {
                if (indexer.api_hits >= indexer.api_hit_limit) continue;
                if (indexer.download_hits >= indexer.download_hit_limit) continue;

                var query = search_params.ToQuery();
                query.Add("indexer", indexer.indexer);
                var query_string = query.ToString();
                if (query_string == null) continue;
                indexer_details.Add(new NzbHydraIndexerDetails()
                {
                    url = new Uri($"{Config.ARGS.hydra}api?{query_string}"),
                    indexer = indexer
                });
            }
        }
        if (indexer_details.Count < 1)
        {
            return Response.Error<NzbHydraIndexers>(ErrorCode.NOT_FOUND, "(NzbHydra|Indexers) No indexers found");
        }
        var indexers = new NzbHydraIndexers()
        {
            indexers = indexer_details,
            search_params = search_params
        };
        return Response.Success(indexers);
    }

    async private static Task<NzbHydraSearchResults> Search(NzbHydraIndexers indexers)
    {
        var urls = indexers.indexers.Select(async indexer =>
        {
            return new NzbHyrdraIndexerSearchItem()
            {
                indexer = indexer,
                results = await RequestGet<NzbHydraResponseSearchResult>(indexer.url.ToString())
            };
        });
        var results = await Task.WhenAll(urls);
        var response_items = new List<NzbHydraResponseItem>();
        var indexer_details = new List<NzbHydraIndexerDetailsResults>();
        foreach (var indexer in results)
        {
            if (indexer.results.error != null)
            {
                indexer_details.Add(new NzbHydraIndexerDetailsResults(indexer.indexer,
                    Response.Error<NzbHydraIndexerResults>(indexer.results.error.code, indexer.results.error.message)));
                continue;
            }
            if (indexer.results.data == null)
            {
                indexer_details.Add(new NzbHydraIndexerDetailsResults(indexer.indexer,
                    Response.Error<NzbHydraIndexerResults>(ErrorCode.UNEXPECTED_ERROR, "(NzbHydra|Search) No error or data")));
                continue;
            }

            var items = indexer.results.data.channel?.item;

            if (items != null)
            {
                response_items.AddRange(items);
                indexer_details.Add(new NzbHydraIndexerDetailsResults(
                            indexer.indexer, Response.Success(new NzbHydraIndexerResults { count = items.Length })));
                continue;
            }
            indexer_details.Add(new NzbHydraIndexerDetailsResults(indexer.indexer,
                Response.Error<NzbHydraIndexerResults>(ErrorCode.UNEXPECTED_ERROR, "(NzbHydra|Search) Invalid data unknown error")));

        }
        var search_results = new NzbHydraSearchResults()
        {
            results = response_items,
            indexers = indexer_details
        };
        return search_results;
    }

    async public static Task<HandleResponse<NzbHydraSearchResults>> TvSearch(int tvdb_id, string? query = null, int? season = null, int? episode = null)
    {
        var categories = new List<int> { 5000, 5030, 5040, 5045, 5010, 5010 };
        if (Config.ARGS.tv_anime) categories.Add(5070);
        if (Config.ARGS.tv_sports) categories.Add(5060);
        var search_params = new NzbHydraSearchParams(
                search_type: "tvsearch",
                categories: categories,
                query,
                tvdb_id,
                tmdb_id: null,
                season,
                episode
        );
        var indexers = await Indexers(search_params);
        if (indexers.error != null)
            return Response.Error<NzbHydraSearchResults>(indexers.error.code, indexers.error.message);
        if (indexers.data == null)
            return Response.Error<NzbHydraSearchResults>(ErrorCode.UNEXPECTED_ERROR, "(NzbHydra|TvSearch) Missing indexers no error or data");
        if (indexers.data.indexers.Count == 0)
            return Response.Error<NzbHydraSearchResults>(ErrorCode.NOT_FOUND, "(NzbHydra|TvSearch) No indexers available");

        var results = await Search(indexers.data);
        return Response.Success(results);
    }

    async public static Task<HandleResponse<NzbHydraSearchResults>> MovieSearch(int tmdb_id, string? query = null)
    {
        var categories = new List<int> { 2000, 2030, 2040, 2050, 2020, 2045, 2080 };
        if (Config.ARGS.movie_foreign) categories.Add(2010);
        if (Config.ARGS.movie_3D) categories.Add(2060);
        var search_params = new NzbHydraSearchParams(
                search_type: "movie",
                categories: categories,
                query,
                tvdb_id: null,
                tmdb_id,
                season: null,
                episode: null
        );
        var indexers = await Indexers(search_params);
        if (indexers.error != null)
            return Response.Error<NzbHydraSearchResults>(indexers.error.code, indexers.error.message);
        if (indexers.data == null)
            return Response.Error<NzbHydraSearchResults>(ErrorCode.UNEXPECTED_ERROR, "(NzbHydra|MovieSearch) Missing indexers no error or data");
        if (indexers.data.indexers.Count == 0)
            return Response.Error<NzbHydraSearchResults>(ErrorCode.NOT_FOUND, "(NzbHydra|MovieSearch) No indexers available");

        var results = await Search(indexers.data);
        return Response.Success(results);
    }
}
