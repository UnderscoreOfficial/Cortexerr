using System.Text;
using Cortexerr.Core.Arrs;
using Cortexerr.Core.Configuration;
using Cortexerr.Core.Errors;
using Cortexerr.Core.Indexers;
using Cortexerr.Core.Logging;
using Cortexerr.Extended.DataStructures;

namespace Cortexerr.Extended.Indexer;

// should release groups be multiple querys? yes
// should searches be merged? yes
//
// targetting logic to maximize options, for usenet we target episodes
//  for debrid we target seasons
//
// ok so usenet episodes poses a unique issue of for a given season how many searches are we doing?
// do I say do searches in the background? like say get the first ep then offload the rest to a background
// process and basically let that continue fetching all the ep searches 

public enum IndexerResultType
{
    JACKETT,
    NZBHYDRA
}

public sealed record IndexerSearchResultItem
{
    public required string name { get; init; }
    public required string link { get; init; }
    public required IndexerResultType type { get; init; }
    public required string indexer { get; init; }
    public required long size { get; init; }
    public DateTimeOffset? upload_date { get; init; }
    public required int files { get; init; }
    public int? torrent_seeders { get; init; }
    public int? torrent_peers { get; init; }
    public int? usenet_grabs { get; init; }
    public int? usenet_episode { get; init; }
    public int? usenet_tvdb_id { get; init; }
    public int? usenet_tmdb_id { get; init; }
}

public sealed record IndexerSearchJob
{
    public required List<List<JackettIndexerDetailsResults>> jackett_indexer_results { get; init; }
    public required List<List<NzbHydraIndexerDetailsResults>> hydra_indexer_results { get; init; }
    public required List<IndexerSearchResultItem> results { get; init; }
}

public sealed record IndexerSearchJobResponse
{
    public required bool finished { get; set; } = false;
    public required IndexerSearchJob indexer_search_job { get; init; }
}

public sealed class Indexer
{
    public List<JackettIndexerSearchResults> jackett_results { get; set; } = new();
    public List<NzbHydraSearchResults> hydra_results { get; set; } = new();
    private int? _total_season_episode_count { get; set; } = null;
    private int? _current_season_episode { get; set; } = 1;

    private string? ReleaseGroups()
    {
        string? response = null;
        if (Config.ARGS.release_groups.Length > 0)
        {
            var string_builder = new StringBuilder();
            foreach (var group in Config.ARGS.release_groups)
            {
                string_builder.Append(group);
                string_builder.Append(" ");
            }
            response = string_builder.ToString();
        }
        return response;
    }

    private IndexerSearchJob MergeResults()
    {
        var jackett_indexer_results = new List<List<JackettIndexerDetailsResults>>();
        var hydra_indexer_results = new List<List<NzbHydraIndexerDetailsResults>>();
        var merge_results = new List<IndexerSearchResultItem>();
        foreach (var jackett in jackett_results)
        {
            jackett_indexer_results.Add(jackett.indexers);
            foreach (var results in jackett.results)
            {
                var name = results.title;
                var link = results.link;
                var indexer = results.jackett_indexer?.text;
                int size = 0;
                if (int.TryParse(results.size, out int size_value)) size = size_value;
                DateTimeOffset? upload_date = null;
                if (results.pub_date != null)
                {
                    Error.Handle(() =>
                    {
                        upload_date = DateTimeOffset.Parse(results.pub_date);
                    });
                }
                int files = 0;
                if (int.TryParse(results.files, out int files_value)) files = files_value;

                int seeders = 0;
                int peers = 0;
                if (results.torznab_attr != null)
                {
                    foreach (var attributes in results.torznab_attr)
                    {
                        if (attributes.name == "seeders")
                        {
                            if (int.TryParse(attributes.value, out int value))
                                seeders = value;
                        }
                        if (attributes.name == "peers")
                        {
                            if (int.TryParse(attributes.value, out int value))
                                peers = value;
                        }
                    }
                }
                if (name == null || link == null || indexer == null)
                    continue;
                merge_results.Add(new IndexerSearchResultItem
                {
                    name = name,
                    link = link,
                    type = IndexerResultType.JACKETT,
                    indexer = indexer,
                    size = size,
                    upload_date = upload_date,
                    files = files,
                    torrent_seeders = seeders,
                    torrent_peers = peers
                });
            }
        }

        foreach (var hydra in hydra_results)
        {
            hydra_indexer_results.Add(hydra.indexers);
            foreach (var results in hydra.results)
            {
                var name = results.title;
                var link = results.link;
                int size = 0;
                if (int.TryParse(results.enclosure?.attributes?.length, out int size_value)) size = size_value;
                int? episode = null;
                int files = 0;
                int grabs = 0;
                int? tvdb_id = null;
                int? tmdb_id = null;
                string? indexer = null;
                DateTimeOffset? upload_date = null;
                if (results.attr != null)
                {
                    foreach (var attribute in results.attr)
                    {
                        var attributes = attribute.attributes;
                        var attr_name = attributes?.name;
                        var attr_value = attributes?.value;

                        if (attr_name == "episode")
                        {
                            if (int.TryParse(attr_value, out int value))
                                episode = value;
                        }
                        if (attr_name == "files")
                        {
                            if (int.TryParse(attr_value, out int value))
                                files = value;
                        }
                        if (attr_name == "grabs")
                        {
                            if (int.TryParse(attr_value, out int value))
                                grabs = value;
                        }
                        if (attr_name == "tvdbid")
                        {
                            if (int.TryParse(attr_value, out int value))
                                tvdb_id = value;
                        }
                        if (attr_name == "tmdbid")
                        {
                            if (int.TryParse(attr_value, out int value))
                                tmdb_id = value;
                        }
                        if (attr_name == "usenetdate")
                        {
                            Error.Handle(() =>
                            {
                                if (attr_value != null)
                                {
                                    upload_date = DateTimeOffset.Parse(attr_value);
                                }
                            });
                        }
                        if (attr_name == "hydraIndexerName")
                        {
                            indexer = attr_value;
                        }
                    }
                }
                if (name == null || link == null || indexer == null)
                    continue;
                merge_results.Add(new IndexerSearchResultItem
                {
                    name = name,
                    link = link,
                    type = IndexerResultType.NZBHYDRA,
                    indexer = indexer,
                    size = size,
                    upload_date = upload_date,
                    files = files,
                    usenet_grabs = grabs,
                    usenet_episode = episode,
                    usenet_tmdb_id = tmdb_id,
                    usenet_tvdb_id = tvdb_id
                });
            }
        }
        var search_job = new IndexerSearchJob
        {
            jackett_indexer_results = jackett_indexer_results,
            hydra_indexer_results = hydra_indexer_results,
            results = merge_results
        };
        return search_job;
    }

    async private Task<HandleResponse<object>> SeriesSearch(
            SonarrResponseSeries sonarr,
            int tvdb_id,
            int? season = null,
            int? episode = null,
            string? target = null
    )
    {
        var jackett_query = sonarr.title_slug?.Replace("-", " ") ?? "";
        if (string.IsNullOrWhiteSpace(target)) jackett_query = $"{jackett_query} {target}";

        HandleResponse<JackettIndexerSearchResults>? jackett = null;
        if (jackett_results.Count == 0)
        {
            jackett = await Jackett.TvSearch(jackett_query, tvdb_id, season, episode);
            if (jackett.data != null)
            {
                jackett_results.Add(jackett.data);
            }
        }
        var hydra = await NzbHydra.TvSearch(tvdb_id, target, season, _current_season_episode);
        if (hydra.data != null)
        {
            hydra_results.Add(hydra.data);
            _current_season_episode++;
        }

        if (jackett?.error != null && hydra.error != null)
        {
            Logger.Log.Error($"[{jackett.error.code}] {jackett.error.message}");
            Logger.Log.Error($"[{hydra.error.code}] {hydra.error.message}");
            return
                Response.Error(ErrorCode.UNEXPECTED_ERROR, "(Indexer|SeriesSearch) Both Jackett & NzbHydra returned unexpected errors");
        }

        if (_current_season_episode > 1 && hydra.error != null)
        {
            return
                Response.Error(hydra.error.code, hydra.error.message);
        }
        return Response.Success();
    }

    async private Task<HandleResponse<object>> MovieSearch(RadarrResponseMovie radarr, int tmdb_id, string? target = null)
    {
        var jackett_query = radarr.sort_title?.Replace("-", " ") ?? "";
        if (string.IsNullOrWhiteSpace(target)) jackett_query = $"{jackett_query} {target}";

        var jackett = await Jackett.MovieSearch(jackett_query, tmdb_id);
        var hydra = await NzbHydra.MovieSearch(tmdb_id, target);
        if (jackett.data != null)
        {
            jackett_results.Add(jackett.data);
        }
        if (hydra.data != null)
        {
            hydra_results.Add(hydra.data);
        }

        if (jackett.error != null && hydra.error != null)
        {
            return
                Response.Error(ErrorCode.UNEXPECTED_ERROR, "(Indexer|MovieSearch) Both Jackett & NzbHydra returned unexpected errors");
        }
        return Response.Success();
    }

    async public Task<HandleResponse<IndexerSearchJobResponse>> Search(RequestJob request_job)
    {
        var ingest = request_job.ingest;
        if (ingest.sonarr != null)
        {
            var sonarr = ingest.sonarr;
            var tvdb_id = ingest.sonarr.request.tvdb_id;
            var season = ingest.sonarr.request.season;
            var episode = ingest.sonarr.request.episode;

            if (_total_season_episode_count == null)
            {
                if (sonarr.series.seasons != null)
                {
                    foreach (var series_season in sonarr.series.seasons)
                    {
                        if (series_season.season_number == sonarr.request.season)
                        {
                            _total_season_episode_count = series_season.statistics?.total_episode_count;
                            break;
                        }
                    }
                }
                if (_total_season_episode_count == null)
                    return Response.Error<IndexerSearchJobResponse>(ErrorCode.INVALID_STATE, "(Indexer|Search) Could not get total season episode count");
            }

            if (_current_season_episode > _total_season_episode_count)
                return Response.Error<IndexerSearchJobResponse>(ErrorCode.ALREADY_EXISTS, "(Indexer|Search) All episodes have already been processed");


            await SeriesSearch(sonarr.series, tvdb_id, season, episode);

            if (Config.ARGS.release_groups.Length > 0)
            {
                foreach (var group in Config.ARGS.release_groups)
                {
                    await SeriesSearch(sonarr.series, tvdb_id, season, episode, group);
                }
            }
            var search_job = MergeResults();
            request_job.indexer_search_jobs.Add(search_job);
            var response = new IndexerSearchJobResponse
            {
                finished = false,
                indexer_search_job = search_job
            };
            if (_current_season_episode > _total_season_episode_count) response.finished = true;
            return Response.Success(response);
        }
        else if (ingest.radarr != null)
        {
            var radarr = ingest.radarr;

            await MovieSearch(radarr.movie, radarr.request.tmdb_id);

            if (Config.ARGS.release_groups.Length > 0)
            {
                foreach (var group in Config.ARGS.release_groups)
                {
                    await MovieSearch(radarr.movie, radarr.request.tmdb_id, group);
                }
            }
            var search_job = MergeResults();
            request_job.indexer_search_jobs.Add(search_job);
            var response = new IndexerSearchJobResponse
            {
                finished = true,
                indexer_search_job = search_job
            };
            return Response.Success(response);
        }
        return
            Response.Error<IndexerSearchJobResponse>(ErrorCode.UNEXPECTED_ERROR, "(Indexer|Search) Ingest missing sonarr & radarr");
    }
}
