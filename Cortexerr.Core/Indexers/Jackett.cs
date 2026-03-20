using System.Web;
using System.Xml;
using Cortexerr.Core.Configuration;
using Cortexerr.Core.Errors;
using Cortexerr.Core.Logging;
using Newtonsoft.Json;

namespace Cortexerr.Core.Indexers;

public class SingleOrArrayConverter<T> : JsonConverter<T[]>
{
    public override T[]? ReadJson(JsonReader reader, Type objectType, T[]? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.StartArray)
        {
            return serializer.Deserialize<T[]>(reader);
        }

        var single = serializer.Deserialize<T>(reader)!;
        return new[] { single };
    }

    public override void WriteJson(JsonWriter writer, T[]? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}

public record JackettResponseXml
{
    [JsonProperty("@version")]
    public string? version { get; init; }
    [JsonProperty("@encoding")]
    public string? encoding { get; init; }
}

// all indexers

public record JackettResponseServer
{
    [JsonProperty("@title")]
    public string? title { get; init; }
}

public record JackettResponseLimits
{
    [JsonProperty("@default")]
    public string? default_limit { get; init; }

    [JsonProperty("@max")]
    public string? max { get; init; }
}

public record JackettResponseSearchMode
{
    [JsonProperty("@available")]
    public string? available { get; init; }

    [JsonProperty("@supportedParams")]
    public string? supported_params { get; init; }
}
public record JackettResponseCapsSearching
{
    public JackettResponseSearchMode? search { get; init; }
    [JsonProperty("tv-search")]
    public JackettResponseSearchMode? tv_search { get; init; }
    [JsonProperty("movie-search")]
    public JackettResponseSearchMode? movie_search { get; init; }
    [JsonProperty("music-search")]
    public JackettResponseSearchMode? music_search { get; init; }
    [JsonProperty("audio-search")]
    public JackettResponseSearchMode? audio_search { get; init; }
    [JsonProperty("book-search")]
    public JackettResponseSearchMode? book_search { get; init; }
}

public record JackettResponseSubCategory
{
    [JsonProperty("@id")]
    public string? id { get; init; }

    [JsonProperty("@name")]
    public string? name { get; init; }
}
public record JackettResponseCategory
{
    [JsonProperty("@id")]
    public string? id { get; init; }

    [JsonProperty("@name")]
    public string? name { get; init; }

    [JsonConverter(typeof(SingleOrArrayConverter<JackettResponseSubCategory>))]
    public JackettResponseSubCategory[]? subcat { get; init; }
}
public record JackettResponseCategories
{
    [JsonConverter(typeof(SingleOrArrayConverter<JackettResponseCategory>))]
    public JackettResponseCategory[]? category { get; init; }
}

public record JackettResponseCaps
{
    public JackettResponseServer? server { get; init; }
    public JackettResponseLimits? limits { get; init; }
    public JackettResponseCapsSearching? searching { get; init; }
    public JackettResponseCategories? categories { get; init; }
}

public record JackettResponseIndexer
{
    [JsonProperty("@id")]
    public string? id { get; init; }
    [JsonProperty("@configured")]
    public string? configured { get; init; }
    public string? title { get; init; }
    public string? description { get; init; }
    public string? link { get; init; }
    public string? language { get; init; }
    public string? type { get; init; }
    public JackettResponseCaps? caps { get; init; }
}

public record JackettResponseIndexers
{
    [JsonConverter(typeof(SingleOrArrayConverter<JackettResponseIndexer>))]
    public JackettResponseIndexer[]? indexer { get; init; }
}

public record JackettResponseAllIndexers
{
    [JsonProperty("?xml")]
    public JackettResponseXml? xml { get; init; }
    public JackettResponseIndexers? indexers { get; init; }
}

// indexer search

public record JackettResponseTorznabAttr
{
    [JsonProperty("@name")]
    public string? name { get; init; }
    [JsonProperty("@value")]
    public string? value { get; init; }
}
public record JackettResponseEnclosure
{
    [JsonProperty("@url")]
    public string? url { get; init; }
    [JsonProperty("@length")]
    public string? length { get; init; }
    [JsonProperty("@type")]
    public string? type { get; init; }
}
public record JackettResponseRssIndexer
{
    [JsonProperty("@id")]
    public string? id { get; init; }
    [JsonProperty("#text")]
    public string? text { get; init; }
}
public record JackettResponseRssItem
{
    public string? title { get; init; }
    public string? guid { get; init; }
    [JsonProperty("jackettindexer")]
    public JackettResponseRssIndexer? jackett_indexer { get; init; }
    public string? type { get; init; }
    public string? comments { get; init; }
    [JsonProperty("pubDate")]
    public string? pub_date { get; init; }
    public string? size { get; init; }
    public string? files { get; init; }
    public string? description { get; init; }
    public string? link { get; init; }
    [JsonConverter(typeof(SingleOrArrayConverter<string>))]
    public string[]? category { get; init; }
    public JackettResponseEnclosure? enclosure { get; init; }
    [JsonProperty("torznab:attr")]
    [JsonConverter(typeof(SingleOrArrayConverter<JackettResponseTorznabAttr>))]
    public JackettResponseTorznabAttr[]? torznab_attr { get; init; }
}

public record JackettResponseAtomLink
{
    [JsonProperty("@href")]
    public string? href { get; init; }
    [JsonProperty("@rel")]
    public string? rel { get; init; }
    [JsonProperty("@type")]
    public string? type { get; init; }
}

public record JackettResponseRssChannel
{
    [JsonProperty("atom:link")]
    public JackettResponseAtomLink? atom_link { get; init; }
    public string? title { get; init; }
    public string? description { get; init; }
    public string? link { get; init; }
    public string? language { get; init; }
    public string? category { get; init; }
    [JsonConverter(typeof(SingleOrArrayConverter<JackettResponseRssItem>))]
    public JackettResponseRssItem[]? item { get; init; }
}

public record JackettResponseRss
{
    [JsonProperty("@version")]
    public string? version { get; init; }
    [JsonProperty("@xmlns:atom")]
    public string? xmlns_atom { get; init; }
    [JsonProperty("@xmlns:torznab")]
    public string? xmlns_torznab { get; init; }
    public JackettResponseRssChannel? channel { get; init; }
}

public record JackettResponseSearchRss
{
    [JsonProperty("?xml")]
    public JackettResponseXml? xml { get; init; }
    public JackettResponseRss? rss { get; init; }
}

public record JackettSearchParams(string search_type, List<int> categories, string? query, int? tvdb_id, int? tmdb_id, int? season, int? episode)
{
    public string ToQueryString()
    {
        var query_string = HttpUtility.ParseQueryString($"apikey={Env.JACKETT_API_KEY}&extended=1&offset=0&limit=100");

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
        return query_string.ToString() ?? "";
    }
}

public record JackettIndexerResults
{
    public int count { get; init; }
}
public record JackettIndexerDetails
{
    public required string id { get; init; }
    public required Uri url { get; init; }
    public JackettSearchParams? search_params { get; init; }
}
public record JackettIndexerDetailsResults(JackettIndexerDetails indexer_details, HandleResponse<JackettIndexerResults> indexer_results);

public record JackettIndexerSearchItem
{
    public required JackettIndexerDetails indexer_details { get; init; }
    public required HandleResponse<JackettResponseSearchRss> rss { get; init; }
}

public record JackettIndexerSearchResults
{
    public required List<JackettResponseRssItem> results { get; init; }
    public required List<JackettIndexerDetailsResults> indexers { get; init; }
}

public static class Jackett
{
    private static readonly HttpClient _client = new()
    {
        BaseAddress = Config.ARGS.jackett
    };
    private static readonly string[] _available_params = { "q", "tvdbid", "tmdbid", "season", "ep" };

    async private static Task<HandleResponse<T>> Request<T>(string endpoint)
    {
        Logger.Log.Debug($"(Jackett|Request) url: ({Config.ARGS.jackett}{endpoint})");
        var handled_response = await Error.HandleAsync<T>(async () =>
        {
            var response = await _client.GetStringAsync(endpoint);
            var xml_doc = new XmlDocument();
            xml_doc.LoadXml(response);
            var json_string = JsonConvert.SerializeXmlNode(xml_doc);
            var json = JsonConvert.DeserializeObject<T>(json_string);
            if (json == null) throw new Exception();
            return json;
        });
        return handled_response;
    }

    async private static Task<HandleResponse<List<JackettIndexerDetails>>> Indexers(JackettSearchParams search_params)
    {
        var endpoint = $"api/v2.0/indexers/all/results/torznab/api?apikey={Env.JACKETT_API_KEY}&t=indexers";
        var request = await Request<JackettResponseAllIndexers>(endpoint);
        if (request.error != null)
            return Response.Error<List<JackettIndexerDetails>>(request.error.code, request.error.message);
        var indexers = request.data?.indexers?.indexer;
        if (indexers == null || indexers.Length < 1)
            return Response.Error<List<JackettIndexerDetails>>(ErrorCode.NOT_FOUND, "(Jackett|Indexers) No indexers found");

        var indexer_details_list = new List<JackettIndexerDetails>();

        foreach (var indexer in indexers)
        {
            if (indexer.id == null) continue;
            string[]? supported_params = null;
            if (!(indexer.configured == "true")) continue;
            if (search_params.search_type == "tvsearch")
            {
                var search = indexer.caps?.searching?.tv_search;
                if (!(search?.available == "yes")) continue;
                supported_params = search?.supported_params?.Split(",");
            }
            else if (search_params.search_type == "movie")
            {
                var search = indexer.caps?.searching?.movie_search;
                if (!(search?.available == "yes")) continue;
                supported_params = search?.supported_params?.Split(",");
            }
            if (supported_params == null) continue;
            var invalid_params = _available_params.Where(param => !supported_params.Contains(param)).ToArray();
            var query = search_params.query;
            var tvdb_id = search_params.tvdb_id;
            var tmdb_id = search_params.tmdb_id;
            var season = search_params.season;
            var episode = search_params.episode;

            foreach (var param in invalid_params)
            {
                switch (param)
                {
                    case "q":
                        query = null;
                        break;
                    case "tvdbid":
                        tvdb_id = null;
                        break;
                    case "tmdbid":
                        tmdb_id = null;
                        break;
                    case "season":
                        season = null;
                        break;
                    case "ep":
                        episode = null;
                        break;
                }
            }
            var avaliable_search_params = new JackettSearchParams(
                search_type: search_params.search_type,
                categories: search_params.categories,
                query,
                tvdb_id,
                tmdb_id,
                season,
                episode
            );
            if (avaliable_search_params.query == null && avaliable_search_params.tvdb_id == null && avaliable_search_params.tmdb_id == null)
                continue;
            var url = new Uri($"{Config.ARGS.jackett}api/v2.0/indexers/{indexer.id}/results/torznab/api?{avaliable_search_params.ToQueryString()}");
            var indexer_details = new JackettIndexerDetails()
            {
                id = indexer.id,
                url = url,
                search_params = avaliable_search_params
            };
            indexer_details_list.Add(indexer_details);
        }
        if (indexer_details_list.Count > 0)
        {
            return Response.Success(indexer_details_list);
        }
        return Response.Error<List<JackettIndexerDetails>>(ErrorCode.NOT_FOUND, "(Jackett|Indexers) No supported indexers found");
    }

    async private static Task<JackettIndexerSearchResults> Search(List<JackettIndexerDetails> indexer_details_list)
    {
        var urls = indexer_details_list.Select(async indexer =>
        {
            return new JackettIndexerSearchItem()
            {
                indexer_details = indexer,
                rss = await Request<JackettResponseSearchRss>(indexer.url.ToString())
            };
        });
        var results = await Task.WhenAll(urls);
        var rss_items = new List<JackettResponseRssItem>();
        var indexer_results = new List<JackettIndexerDetailsResults>();
        foreach (var indexer in results)
        {
            if (indexer.rss.error != null)
            {
                indexer_results.Add(new JackettIndexerDetailsResults(
                            indexer.indexer_details,
                            Response.Error<JackettIndexerResults>(indexer.rss.error.code, indexer.rss.error.message)));
                continue;
            }
            if (indexer.rss.data == null)
            {
                indexer_results.Add(new JackettIndexerDetailsResults(
                            indexer.indexer_details,
                            Response.Error<JackettIndexerResults>(ErrorCode.UNEXPECTED_ERROR, "(Jackett|Search) No error or data")));
                continue;
            }

            var channel = indexer.rss.data?.rss?.channel;
            var items = channel?.item;

            // need to verify full but I think this case would mean valid response just no items
            if (channel != null && items == null)
            {
                indexer_results.Add(new JackettIndexerDetailsResults(
                            indexer.indexer_details,
                            Response.Success(new JackettIndexerResults { count = 0 })));
                continue;
            }
            if (items != null)
            {
                indexer_results.Add(new JackettIndexerDetailsResults(
                            indexer.indexer_details,
                            Response.Success(new JackettIndexerResults { count = items.Length })));
                if (items.Length > 0)
                {
                    rss_items.AddRange(items);
                }
                continue;
            }
            indexer_results.Add(new JackettIndexerDetailsResults(
                        indexer.indexer_details,
                        Response.Error<JackettIndexerResults>(ErrorCode.UNEXPECTED_ERROR, "(Jackett|Search) Invalid data unknown error")));
        }
        var search_results = new JackettIndexerSearchResults()
        {
            results = rss_items,
            indexers = indexer_results
        };
        return search_results;
    }

    async public static Task<HandleResponse<JackettIndexerSearchResults>> TvSearch(string query, int tvdb_id, int? season = null, int? episode = null)
    {
        if (string.IsNullOrEmpty(query))
            return Response.Error<JackettIndexerSearchResults>(ErrorCode.INVALID_INPUT, "(Jackett|TvSearch) Invalid query");

        var categories = new List<int> { 5000, 5030, 5040, 5045, 5010, 5010 };
        if (Config.ARGS.tv_anime) categories.Add(5070);
        if (Config.ARGS.tv_sports) categories.Add(5060);
        var search_params = new JackettSearchParams(
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
            return Response.Error<JackettIndexerSearchResults>(indexers.error.code, indexers.error.message);
        if (indexers.data == null)
            return Response.Error<JackettIndexerSearchResults>(ErrorCode.UNEXPECTED_ERROR, "(Jackett|TvSearch) Missing indexers no error or data");
        if (indexers.data.Count == 0)
            return Response.Error<JackettIndexerSearchResults>(ErrorCode.NOT_FOUND, "(Jackett|TvSearch) No indexers available");

        var results = await Search(indexers.data);
        return Response.Success(results);
    }

    async public static Task<HandleResponse<JackettIndexerSearchResults>> MovieSearch(string query, int tmdb_id)
    {
        if (string.IsNullOrEmpty(query))
            return Response.Error<JackettIndexerSearchResults>(ErrorCode.INVALID_INPUT, "(Jackett|MovieSearch) Invalid query");

        var categories = new List<int> { 2000, 2030, 2040, 2050, 2020, 2045, 2080 };
        if (Config.ARGS.movie_foreign) categories.Add(2010);
        if (Config.ARGS.movie_3D) categories.Add(2060);
        var search_params = new JackettSearchParams(
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
            return Response.Error<JackettIndexerSearchResults>(indexers.error.code, indexers.error.message);
        if (indexers.data == null)
            return Response.Error<JackettIndexerSearchResults>(ErrorCode.UNEXPECTED_ERROR, "(Jackett|MovieSearch) Missing indexers no error or data");
        if (indexers.data.Count == 0)
            return Response.Error<JackettIndexerSearchResults>(ErrorCode.NOT_FOUND, "(Jackett|MovieSearch) No indexers available");

        var results = await Search(indexers.data);
        return Response.Success(results);
    }
}
