using System.Text.RegularExpressions;
using Cortexerr.Decisions.DataStructures;
using Cortexerr.Extended.DataStructures;
using Cortexerr.Extended.Indexer;

namespace Cortexerr.Decisions.Logic.Filter;

public static class RequestMatching
{
  public static DecisionLogicJob Filter(RequestJob request_job, IndexerSearchJob search_job)
  {
    var sonarr = request_job.ingest.sonarr;
    var radarr = request_job.ingest.radarr;
    var results = new List<IndexerSearchResultItem>();
    var bad_names = new List<string>();

    var language_match = @"\b(german|french|spanish|italian|portuguese|dutch|russian|japanese|korean|chinese|hindi|arabic|turkish|polish|swedish|norwegian|danish|finnish|hebrew|czech|hungarian|romanian|greek|thai|vietnamese|multi|vostfr|ita|ger|fre|spa|por|dut|rus|jap|kor|chi)\b";

    foreach (var job in search_job.indexer_search_job.results)
    {
      // all name based filtering is only used for torrents usenet exclusively uses ids
      if (sonarr != null)
      {
        if (job.usenet_tvdb_id != null)
        {
          if (job.usenet_tvdb_id == sonarr.request.tvdb_id)
          {
            results.Add(job);
            continue;
          }
        }
        var sort_title = sonarr.series.sort_title;
        if (sort_title == null) continue;

        var name = job.name.ToLowerInvariant();

        var ep_season_regex = @"(?:s(\d{1,2})|season\s*(\d+))(?:e(\d{1,2}))?|(\d{1,2})x(\d{1,2})|episodes?\s*(\d+)|ep\.?\s*(\d+)|e(\d{1,2})";
        var main_match = Regex.Matches(name, ep_season_regex);

        var total_found_season = 0;
        var total_found_episodes = 0;
        var invalid_episode = false;
        var valid_seasons = false;
        foreach (Match match in main_match)
        {
          // Console.WriteLine("Match: " + match);
          // for (var i = 0; i < match.Groups.Count; i++)
          // {
          //   Console.WriteLine($"Group_{i}: " + match.Groups[i]);
          // }
          var episode_match = match.Groups[3].Success ? match.Groups[3].Value
                        : match.Groups[5].Success ? match.Groups[5].Value
                        : match.Groups[6].Success ? match.Groups[6].Value
                        : match.Groups[7].Success ? match.Groups[7].Value
                        : match.Groups[8].Value;

          if (int.TryParse(episode_match, out int episode_number))
          {
            if (episode_number != search_job.target.episode)
            {
              invalid_episode = true;
            }
            Console.WriteLine("Episode: " + episode_number);
            total_found_episodes++;
          }

          var season_match = match.Groups[1].Success ? match.Groups[1].Value
               : match.Groups[2].Success ? match.Groups[2].Value
               : match.Groups[4].Value;
          if (int.TryParse(season_match, out int season_number))
          {
            if (season_number == sonarr.request.season) valid_seasons = true;
            Console.WriteLine("Season " + season_number);
            total_found_season++;
          }
        }
        Console.WriteLine(name);
        // should allow multiple episodes with invalid_episodes to be considered as pack episodes
        // while allowing all without any ep though only passing with 1 ep and its wrong
        if (total_found_episodes == 1 && invalid_episode) continue;

        var pack_range_match = @"\bs\d{1,2}\s*[-]\s*s\d{1,2}\b|(?:s\d{1,2}\s*){2,}";
        var pack_keyword_match = @"\b(complete|full\s*series|the\s*complete\s*series|complete\s*series|complete\s*season)\b";
        var season_pack = Regex.IsMatch(name, pack_range_match) || Regex.IsMatch(name, pack_keyword_match);
        if (season_pack) valid_seasons = true;
        if (total_found_season > 1) valid_seasons = true; // treating multiple seasons as packs even if not valid requested season

        if (!valid_seasons) continue;

        // temporary to avoid non default languges need to decide how to target this ideally config option array to enable extra languages ideally 
        // it would just share both for subs and audio.
        if (Regex.IsMatch(name, language_match)) continue;

        var fuzzy_input = Regex.Replace(name, @"[._]", " ");
        fuzzy_input = Regex.Replace(fuzzy_input, @"\s+", " ").Trim();

        var fuzzy_token_output = FuzzySharp.Fuzz.TokenSetRatio(fuzzy_input, sort_title);
        var fuzzy_ratio_output = FuzzySharp.Fuzz.Ratio(fuzzy_input, sort_title);

        if (fuzzy_token_output >= 85 && fuzzy_ratio_output >= 60)
        {
          var name_with_year = $"{sort_title} {sonarr.series.year}";
          fuzzy_token_output = FuzzySharp.Fuzz.TokenSetRatio(fuzzy_input, name_with_year);
          fuzzy_ratio_output = FuzzySharp.Fuzz.Ratio(fuzzy_input, name_with_year);

          if (fuzzy_token_output >= 95 && fuzzy_ratio_output >= 75)
          {
            results.Add(job);
          }
        }
        else if (fuzzy_token_output >= 85)
        {
          results.Add(job);
        }
      }
      if (radarr != null)
      {
        var sort_title = radarr.movie.sort_title;
        if (sort_title == null) continue;

        var name = job.name.ToLowerInvariant();

        // temporary to avoid non default languges need to decide how to target this ideally config option array to enable extra languages ideally 
        // it would just share both for subs and audio.
        if (Regex.IsMatch(name, language_match)) continue;

        var fuzzy_input = Regex.Replace(name, @"[._]", " ");
        fuzzy_input = Regex.Replace(fuzzy_input, @"\s+", " ").Trim();

        var fuzzy_token_output = FuzzySharp.Fuzz.TokenSetRatio(fuzzy_input, sort_title);
        var fuzzy_ratio_output = FuzzySharp.Fuzz.Ratio(fuzzy_input, sort_title);

        if (fuzzy_token_output >= 85 && fuzzy_ratio_output >= 60)
        {
          var name_with_year = $"{sort_title} {radarr.movie.year}";
          fuzzy_token_output = FuzzySharp.Fuzz.TokenSetRatio(fuzzy_input, name_with_year);
          fuzzy_ratio_output = FuzzySharp.Fuzz.Ratio(fuzzy_input, name_with_year);

          if (fuzzy_token_output >= 95 && fuzzy_ratio_output >= 75)
          {
            results.Add(job);
          }
        }
        else if (fuzzy_token_output >= 85)
        {
          results.Add(job);
        }
      }
    }
    return new DecisionLogicJob { request_job = request_job, search_job = search_job, results = results };
  }
}
