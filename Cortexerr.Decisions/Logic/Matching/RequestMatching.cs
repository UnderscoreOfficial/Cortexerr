using Cortexerr.Core.Logging;
using Cortexerr.Extended.DataStructures;
using Cortexerr.Extended.Indexer;

namespace Cortexerr.Decisions.Logic.Matching;

public sealed record DecisionLogicMatchedItem
{
  public bool high_dynamic_range { get; init; }
  public bool dolby_vision { get; init; }
  public required string[] languages { get; init; }
  public string? release_group { get; init; }
  public required RequestReleaseNameMatchingResults fuzzy_name { get; init; }
  public RequestSeriesMatchingResults? series { get; init; }
  public Resolution? resolution { get; init; }
  public AudioCodec? audio_codec { get; init; }
  public VideoCodec? video_codec { get; init; }
  public RipType? riptype { get; init; }

}
public sealed record DecisionLogicRequestMatching
{
  public required DecisionLogicMatchedItem matched { get; init; }
  public required IndexerSearchResultItem item { get; init; }
}
public sealed record DecisionLogicMatchingJob
{
  public required RequestJob request_job { get; init; }
  public required IndexerSearchJob search_job { get; init; }
  public required List<DecisionLogicRequestMatching> results { get; init; }
}

public static class RequestMatching
{
  public static DecisionLogicMatchingJob Match(RequestJob request_job, IndexerSearchJob search_job)
  {
    var sonarr = request_job.ingest.sonarr;
    var radarr = request_job.ingest.radarr;
    var results = new List<DecisionLogicRequestMatching>();

    foreach (var job in search_job.indexer_search_job.results)
    {
      if (sonarr != null)
      {
        var sort_title = sonarr.series.sort_title;
        if (sort_title == null)
        {
          Logger.Log.Error("(RequestMatching|Match) Missing series sort title");
          continue;
        }
        var name = job.name.ToLowerInvariant();

        var series_match = RequestSeriesMatching.Match(request_job, search_job, name);
        var language_match = RequestLanguageMatching.Match(request_job, search_job, name);
        var release_name_match = RequestReleaseNameMatching.Match(request_job, search_job, name);
        var resolution_match = RequestResolutionMatching.Match(request_job, search_job, name);
        var high_dynamic_range_match = RequestHighDynamicRangeMatching.Match(request_job, search_job, name);
        var dolby_vision_match = RequestDolbyVisionMatching.Match(request_job, search_job, name);
        var audio_codec_match = RequestAudioCodecMatching.Match(request_job, search_job, name);
        var video_codec_match = RequestVideoCodecMatching.Match(request_job, search_job, name);
        var riptype_match = RequestRipTypeMatching.Match(request_job, search_job, name);
        var release_group_match = RequestReleaseGroupMatching.Match(request_job, search_job, name);

        var decision_logic_matched = new DecisionLogicMatchedItem
        {
          high_dynamic_range = high_dynamic_range_match,
          dolby_vision = dolby_vision_match,
          languages = language_match,
          release_group = release_group_match,
          fuzzy_name = release_name_match,
          series = series_match,
          resolution = resolution_match,
          audio_codec = audio_codec_match,
          video_codec = video_codec_match,
          riptype = riptype_match
        };
        var request_matching = new DecisionLogicRequestMatching
        {
          matched = decision_logic_matched,
          item = job
        };
        results.Add(request_matching);
      }
      else if (radarr != null)
      {
        var sort_title = radarr.movie.sort_title;
        if (sort_title == null)
        {
          Logger.Log.Error("(RequestMatching|Match) Missing movie sort title");
          continue;
        }
        var name = job.name.ToLowerInvariant();

        var language_match = RequestLanguageMatching.Match(request_job, search_job, name);
        var release_name_match = RequestReleaseNameMatching.Match(request_job, search_job, name);
        var resolution_match = RequestResolutionMatching.Match(request_job, search_job, name);
        var high_dynamic_range_match = RequestHighDynamicRangeMatching.Match(request_job, search_job, name);
        var dolby_vision_match = RequestDolbyVisionMatching.Match(request_job, search_job, name);
        var audio_codec_match = RequestAudioCodecMatching.Match(request_job, search_job, name);
        var video_codec_match = RequestVideoCodecMatching.Match(request_job, search_job, name);
        var riptype_match = RequestRipTypeMatching.Match(request_job, search_job, name);
        var release_group_match = RequestReleaseGroupMatching.Match(request_job, search_job, name);

        var decision_logic_matched = new DecisionLogicMatchedItem
        {
          high_dynamic_range = high_dynamic_range_match,
          dolby_vision = dolby_vision_match,
          languages = language_match,
          release_group = release_group_match,
          fuzzy_name = release_name_match,
          resolution = resolution_match,
          audio_codec = audio_codec_match,
          video_codec = video_codec_match,
          riptype = riptype_match
        };
        var request_matching = new DecisionLogicRequestMatching
        {
          matched = decision_logic_matched,
          item = job
        };
        results.Add(request_matching);
      }
    }
    return new DecisionLogicMatchingJob { request_job = request_job, search_job = search_job, results = results };
  }
}
