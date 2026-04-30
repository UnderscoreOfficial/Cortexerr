using System.Text.Json;
using Cortexerr.Decisions.Logic.Matching;

namespace Cortexerr.Decisions.Logic.Filtering;

public static class RequestFiltering
{
  public static DecisionLogicMatchingJob Filter(DecisionLogicMatchingJob decision_logic_matching_job)
  {
    var sonarr = decision_logic_matching_job.request_job.ingest.sonarr;
    var radarr = decision_logic_matching_job.request_job.ingest.radarr;
    var results = new List<DecisionLogicRequestMatching>();

    foreach (var job in decision_logic_matching_job.results)
    {
      if (sonarr != null)
      {
        var invalid_episode = false;
        var valid_season = false;
        var episodes = job.matched.series?.episodes;
        var seasons = job.matched.series?.seasons;

        var target_episode = decision_logic_matching_job.search_job.target.episode;
        var episodes_length = episodes?.Length ?? 0;
        if (target_episode.HasValue && !episodes.Contains(target_episode.Value))
        {
          invalid_episode = true;
        }
        // should allow multiple episodes with invalid_episodes to be considered as pack episodes
        // while allowing all without any ep though only passing with 1 ep and its wrong
        if (episodes_length == 1 && invalid_episode)
          continue;

        var target_season = decision_logic_matching_job.search_job.target.season;
        var seasons_length = seasons?.Length ?? 0;
        if (target_season.HasValue && seasons.Contains(target_season.Value))
        {
          valid_season = true;
        }

        var pack = job.matched.series?.pack;


        // pack states have to be somewhat strict, for season packs it must always include the season irrelevant of if pack is true or not
        if (valid_season && episodes_length == 0)
        {
          // season pack
          job.matched.series?.pack = PackType.SEASON;
        }
        else if (episodes_length > 1)
        {
          // episode pack (very niche and mixed could be for multi part series and or parts of season packs)
          job.matched.series?.pack = PackType.EPISODE;
        }
        else if (seasons_length > 1 || pack == PackType.FULL)
        {
          // full pack
          job.matched.series?.pack = PackType.FULL;
        }

        // temporary to avoid non default languges need to decide how to target this ideally config option array to enable extra languages ideally 
        // it would just share both for subs and audio.
        if (job.matched.languages?.Length > 0)
        {
          continue;
        }


        // uggh temporarly ignore way worse false positives need to rethink a better aproach 
        // if (job.matched.fuzzy_name.token_set_ratio >= 85 && job.matched.fuzzy_name.ratio >= 60)
        // {
        //   if (job.matched.fuzzy_name.token_set_ratio_with_year >= 95 && job.matched.fuzzy_name.ratio_with_year >= 75)
        //   {
        //     results.Add(job);
        //   }
        //   else
        //   {
        //     Console.WriteLine("Not added yr: " + job.item.name);
        //     Console.WriteLine("token: " + job.matched.fuzzy_name.token_set_ratio);
        //     Console.WriteLine("ratio: " + job.matched.fuzzy_name.ratio);
        //     Console.WriteLine("token_year: " + job.matched.fuzzy_name.token_set_ratio_with_year);
        //     Console.WriteLine("ratio_year: " + job.matched.fuzzy_name.ratio_with_year);
        //   }
        // }
        if (job.matched.fuzzy_name.token_set_ratio >= 85)
        {
          results.Add(job);
        }
        else
        {
          Console.WriteLine("Not added: " + job.item.name);
        }
      }
      if (radarr != null)
      {
        var sort_title = radarr.movie.sort_title;
        if (sort_title == null) continue;

        // temporary to avoid non default languges need to decide how to target this ideally config option array to enable extra languages ideally 
        // it would just share both for subs and audio.
        if (job.matched.languages?.Length > 0) continue;

        if (job.matched.fuzzy_name.token_set_ratio >= 85 && job.matched.fuzzy_name.ratio >= 60)
        {
          if (job.matched.fuzzy_name.token_set_ratio_with_year >= 95 && job.matched.fuzzy_name.ratio_with_year >= 75)
          {
            results.Add(job);
          }
        }
        else if (job.matched.fuzzy_name.token_set_ratio >= 85)
        {
          results.Add(job);
        }
      }
    }
    return new DecisionLogicMatchingJob
    {
      request_job = decision_logic_matching_job.request_job,
      search_job = decision_logic_matching_job.search_job,
      results = results
    };
  }
}
