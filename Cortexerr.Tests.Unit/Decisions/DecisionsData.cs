using Cortexerr.Core.Arrs;
using Cortexerr.Core.Indexers;
using Cortexerr.Core.Ingest;
using Cortexerr.Core.Utilities;
using Cortexerr.Decisions.Logic.Matching;
using Cortexerr.Extended.DataStructures;
using Cortexerr.Extended.Indexer;

namespace Cortexerr.Tests.Unit.Decisions;

public sealed record DecisionsDataSort
{
    public required DecisionLogicRequestMatching x { get; init; }
    public required DecisionLogicRequestMatching y { get; init; }
}

public static class DecisionsData
{
    public static DecisionLogicMatchingJob Match(string[] names, int? size = null)
    {
        var request = new IngestSonarrRequest
        {
            length = size == null ? 1000 : (int)size,
            release = "Season.1.264.Webdl",
            rid = 123,
            tvdb_id = 123,
            season = 1
        };
        var request_job = new RequestJob
        {
            ingest = new Ingest
            {
                hash = Utils.RandomHexadecimal(40),
                sonarr = new IngestSonarr
                {
                    series = new SonarrResponseSeries
                    {
                        id = 123,
                        title = "Generic Test Show Name",
                        sort_title = "Generic Test Show Name",
                        seasons = new[] {
                            new SonarrResponseSeason {
                                season_number = 1,
                                statistics = new SonarrResponseSeasonStatistics {
                                    total_episode_count = 8
                                }}
                        },
                    },
                    request = request
                },
                status = new IngestStatus
                {

                    completed = false,
                    download_speed = 0,
                    eta = 0,
                    name = "Generic Test Show Name",
                    progress = 0,
                    save_path = "/",
                    size = size == null ? 1000 : (int)size,
                    state = TorrentState.DOWNLOADING
                }
            }
        };
        var result_item = new List<IndexerSearchResultItem>();
        foreach (var item in names)
        {
            result_item.Add(new IndexerSearchResultItem
            {
                name = item,
                size = size == null ? Utils.RandomByteSize(100) : (int)size,
                files = 1,
                indexer = "",
                link = "",
                type = IndexerResultType.JACKETT,
                torrent_peers = 10,
                torrent_seeders = 20,
            });
        }
        var search_job_results = new IndexerSearchJobResults
        {
            jackett_indexer_results = new List<List<JackettIndexerDetailsResults>> { }, // mainly for stat tracking safe to skip
            hydra_indexer_results = new List<List<NzbHydraIndexerDetailsResults>> { }, // --- above --
            results = result_item,
        };

        var search_job = new IndexerSearchJob
        {
            finished = false,
            target = new IndexerSearchJobTarget
            {
                season = 1,
                episode = 4
            },
            indexer_search_job = search_job_results
        };
        var matched = Matching.Match(request_job, search_job);
        return matched;
    }
    public static DecisionLogicMatchingJob Data()
    {
        string[] names = {
        // [+]  Generic Test Show Name S01 - should PASS
        "Generic.Test.Show.Name.S01E01.720p.BluRay.x264-REWARD",
        "Generic.Test.Show.Name.S01E02.HDTV.x264-KILLERS[eztv]",
        "Generic.Test.Show.Name.S01E03.720p.HDTV.x264-LOL[ettv]",
        "Generic.Test.Show.Name.S01E04.HDTV.x264-LOL[eztv]",
        "Generic.Test.Show.Name.S01E05.720p.BluRay.x264",
        "Generic.Test.Show.Name.S01E06.1080p.WEB-DL.DD5.1.H264",
        "Generic.Test.Show.Name.S01E07.HDTV.x264-KILLERS[rartv]",
        "Generic Test Show Name S01E01 REPACK 720p HDTV x264-KILLERS",
        "Generic Test Show Name S01E01 iNTERNAL 720p WEB h264",
        "Generic Test Show Name S01E01 480p x264-mSD",
        "Generic Test Show Name S01E01 1080p WEB-DL x265",
        "Generic.Test.Show.Name.S01E01.REPACK.HDTV.x264-KILLERS[eztv]",
        "Generic Test Show Name S01E01(1080p x265-sharpy)",
        "generic.test.show.name.s01e01.hdtv.x264-killers",
        "Generic Test Show Name S01E02 720p HDTV x264",
        "GENERIC.TEST.SHOW.NAME.S01E03.HDTV.x264[eztv]",
        "Generic.Test.Show.Name.1x01.720p.BluRay.x264",
        "Generic.Test.Show.Name.1x02.HDTV.x264",
        "Generic.Test.Show.Name.1x07.720p.WEB-DL",
        "Generic Test Show Name (2005) S01E01 1080p BluRay",
        "Generic Test Show Name 2005 S01E01 720p",
        "Generic.Test.Show.Name.S01E01.Pilot.1080p.BluRay.REMUX.AVC.DTS-HD.MA.5.1",
        "Generic.Test.Show.Name.S01E01.Pilot.720p.WEB-DL.DD5.1.H264-Coo7[rartv]",
        "Generic Test Show Name S01E01 Pilot 1080p NF WEB-DL DDP5.1 H264",

        // [+]  Generic Test Show Name S01 PACKS - should PASS
        "Generic Test Show Name S01 Complete 1080p BluRay x265 HEVC",
        "Generic.Test.Show.Name.S01.1080p.BluRay.x264-SHORTBREHD[rartv]",
        "Generic.Test.Show.Name.S01.720p.BluRay.X264-REWARD[rartv]",
        "Generic Test Show Name Season 1 Complete BDRip x265",
        "Generic Test Show Name Season 1 1080p BDRip x265 HEVC 10bit AAC",
        "Generic Test Show Name S01 1080p BluRay x265 HEVC 6CH",
        "generic.test.show.name.s01.complete.webrip.x264",
        "Generic Test Show Name (2005) Season 1 S01 1080p BDRip",
        "Generic.Test.Show.Name.S01.BDRip.X264-REWARD[rartv]",
        "Generic Test Show Name S01 720p BluRay X264",

        // [+] Generic Test Show Name FULL SERIES PACKS - should PASS
        "Generic Test Show Name Complete Series 1080p BluRay x265",
        "Generic.Test.Show.Name.The.Complete.Series.BluRay.x264",
        "Generic Test Show Name S01-S05 Complete 1080p",
        "Generic Test Show Name S01 S02 S03 S04 S05 1080p BluRay",
        "Generic Test Show Name Full Series BDRip x265 HEVC",
        "Generic.Test.Show.Name.Complete.Collection.1080p.BluRay",
        "Generic Test Show Name - The Complete Series (2005-2010) 1080p",
        "Generic Test Show Name S01-05 720p BluRay X264-REWARD",

        // [X] Generic Test Show Name WRONG SEASON - should DROP
        "Generic.Test.Show.Name.S02E01.720p.BluRay.x264",
        "Generic Test Show Name S02 Complete 1080p BluRay",
        "Generic.Test.Show.Name.S03E01.HDTV.x264-LOL",
        "Generic Test Show Name Season 2 BDRip x265",
        "Generic.Test.Show.Name.S04.1080p.BluRay.x264",
        "Generic Test Show Name S05E01 720p WEB-DL",
        "Generic.Test.Show.Name.S02.720p.BluRay.X264-REWARD",
        "Generic Test Show Name Season 4 Complete",
        "Generic.Test.Show.Name.2x01.720p.BluRay",
        "Generic Test Show Name S03 1080p BluRay x265 HEVC",

        // [X] Generic Test Show Name LANGUAGE TAGS - should DROP
        "generic.test.show.name.s01.complete.french.webrip.x264",
        "Generic.Test.Show.Name.S01E01.German.1080p.WebHD.x264",
        "Generic.Test.Show.Name.S01.MULTi.1080p.BluRay.x264",
        "Generic.Test.Show.Name.S01E01.VOSTFR.BluRay.x264",
        "generic.test.show.name.s01e02.spanish.720p.hdtv",
        "Generic Test Show Name S01E01 ITA ENG 1080p WEB-DL",
        "Generic.Test.Show.Name.S01.RUSSIAN.BDRip.x264",
        "Generic Test Show Name S01E01 720p HDTV x264 GER",
        "generic.test.show.name.s01e01.italian.webrip",
        "Generic.Test.Show.Name.S01.PORTUGUESE.WEBRip",

        // [X] COMPLETELY DIFFERENT SHOWS - should DROP via fuzzy
        "Crimson.Drift.Circle.S01E01.720p.BluRay.x264",
        "Amber Fold Square S01 Complete 1080p BluRay",
        "Glowing.Pivot.Oval.S01E01.720p.BluRay.x264-REWARD",
        "Cobalt Spin Triangle Season 1 Complete BDRip",
        "Marble.Twist.Cube.S01E01.720p.BluRay.x264",
        "Violet Surge Prism S01 Complete 1080p",
        "Teal.Drift.Hexagon.S01E01.720p.BluRay.x264",
        "Neon Fold Diamond Season 1 Complete BluRay",
        "Rustic.Leap.Ellipse.S01E01.720p.BluRay.x264",
        "Slate Spin Rhombus S01 Complete 1080p BDRip",
        "Bronze.Twist.Pentagon.S01E01.HDTV.x264",
        "Ivory.Surge.Wedge.S01E01.1080p.BluRay",
        "Blaze.Drift.Arc.S01E01.720p.BluRay.x264",
        "Moss.Fold.Spiral.S01E01.HDTV",
        "Cinder.Leap.Cone.S01E01.1080p",
        "Frost.Spin.Bolt.S01E01.720p.BluRay",
        "Amber.Surge.Ring.S01E01.720p.BluRay.x264",
        "Cobalt.Fold.Arch.S01E01.720p",
        "Marble.Drift.Wedge.S01E01.1080p.WEB-DL",
        "Olive.Twist.Slab.S01E01.720p.WEB-DL.x264",
        "Crimson.Leap.Torus.S01E01.720p.WEB-DL",
        "Teal.Pivot.Dome.S01E01.1080p.WEB",
        "Violet.Drift.Knot.S01E01.720p.BluRay",
        "Slate.Surge.Coil.S01E01.720p.WEB-DL.German",
        "Rustic.Fold.Mesh.S01E01.720p.WEB",
        "Ivory.Leap.Grid.S01E01.1080p",
        "Neon.Spin.Spike.S01E01.720p.BluRay",
        "Bronze.Drift.Shell.S01E01.1080p.BluRay",
        "Frost.Twist.Beam.S01E01.720p",
        "Blaze.Pivot.Stripe.S01E01.720p.BluRay.x264",

        // [X] ABBREVIATIONS - should DROP via fuzzy
        "GTSN.S01E01.720p.BluRay.x264",
        "G.Test.S01E01.HDTV.x264",
        "Gnr.Tst.S01E01.720p",
        "GnTs.S01E01.1080p.WEB",

        // [X] GARBAGE / MALFORMED - should DROP
        "S01E01.720p.BluRay.x264-REWARD",
        "1080p.BluRay.x264-REWARD",
        "720p.HDTV.x264-LOL[eztv]",
        "Complete.Series.1080p.BluRay",
        "x264-KILLERS[eztv]",
        "HDTV.x264-LOL",
        "[eztv].720p.BluRay",
        "Season 1 Complete",
        "S01E01",
        "1080p.WEB-DL.DD5.1.H264",

        // [X] SIMILAR BUT WRONG SHOW - tricky fuzzy edge cases
        "Generic Test Show Null S01E01 720p BluRay",        // last word swapped
        "Generic Test Show Point S01E01 HDTV x264",         // last word swapped
        "Generic Test Show Edge S01E01 WEB-DL",             // last word swapped
        "Generic Test Show Void S01E01 720p",               // last word swapped
        "Name Show Test Generic S01E01 HDTV",               // reversed word order
        "Generic Test Show Name Extra S01E01 720p",         // extra word appended
        "Generic Test Show Name Habits S01E01 WEB",         // extra word appended
        "Generic.Test.Show.Name.Files.S01E01.720p",         // extra word appended
        "Very Test Show Stuff S01E01 1080p WEB",            // first and last word swapped
        "Test Show S01E01 720p BluRay x264",                // truncated title

        // [X] NO SEASON INFO AT ALL - should DROP
        "Generic Test Show Name 1080p BluRay x265 Complete",
        "Generic.Test.Show.Name.720p.BluRay.x264-REWARD",
        "Generic Test Show Name WEB-DL x264",
        "Generic.Test.Show.Name.HDTV.x264-LOL",
        "Generic Test Show Name BDRip x265 HEVC",

        // [?] EDGE CASES - interesting boundary scenarios
        "Generic.Test.Show.Name.S01E01.Pilot.720p.WEB-DL.DD5.1.H264-Coo7",   // episode title in name
        "Generic Test Show Name - Season 1 (2005) 1080p Remux",               // dash separator
        "Generic Test Show Name [S01] 720p BluRay",                           // brackets around season
        "Generic_Test_Show_Name_S01E01_720p_BluRay",                          // underscores throughout
        "Generic Test Show Name S01E01-E07 720p BluRay",                      // episode range
        "Generic.Test.Show.Name.S01E01E02.720p.BluRay",                       // multi episode attached
        "Generic Test Show Name S01 02 720p H265 AAC",                        // space between S01 and 02
        "Generic Test Show Name (Season 1) 1080p BluRay",                     // parentheses around season
        "[SubGroup] Generic Test Show Name S01E01 720p BluRay",               // leading subgroup tag
        "Generic Test Show Name S01E01 [1080p] [BluRay] [x265]",             // brackets around quality
        "Generic.Test.Show.Name.S01.Complete-REWARD[rartv]",                  // complete flag mid-name
        "Generic Test Show Name 2005 Season 1 Complete 1080p BDRip x265",    // year before season word

        // [+] episode naming conventions
        "Generic Test Show Name Season 1 episode 5 Complete 1080p BDRip x265",    // written-out episode word
        "Generic Test Show Name Season 01 episode 02 Complete 1080p BDRip x265",  // zero-padded season+ep
        "Generic Test Show Name Season 01 ep 07 Complete 1080p BDRip x265",       // abbreviated ep
        "Generic Test Show Name Season 01 ep 7 Complete 1080p BDRip x265",        // abbreviated ep no pad
        "Generic Test Show Name Season 1 episode 5 1080p BDRip x265",    // written-out episode word
        "Generic Test Show Name Season 01 episode 02 1080p BDRip x265",
        "Generic Test Show Name Season 01 ep 07 1080p BDRip x265",
        "Generic Test Show Name Season 01 ep 7 1080p BDRip x265",

        // [?] hdr, dolby vision 3 hdr 4 dv
        "Generic Test Show Name Season S01E01 hdr 1080p BDRip x265",
        "Generic Test Show Name Season S01E01 dv 1080p BDRip x265",
        "Generic Test Show Name Season S01E01 dolby vision 1080p BDRip x265",
        "Generic Test Show Name Season S01E01 dv hdr 1080p BDRip x265",
        "Generic Test Show Name Season S01E01 dolby vision hdr 1080p BDRip x265",
        };
        return Match(names);
    }
    public static List<string> MatchBuilder(string[] matches)
    {
        var show_name = "Generic Test Show Name";
        var optional_cases = new[] { "", " ", ":", "-" };
        var names = new List<string>();
        foreach (var match in matches)
        {
            var match_items = match.Split(" ");
            if (match_items.Length == 1)
            {
                names.Add($"{show_name} {match}");
                continue;
            }
            foreach (var optional in optional_cases)
            {
                names.Add($"{show_name} {string.Join(optional, match_items)}");
            }
        }
        return names;
    }
    public static DecisionLogicMatchingJob NormalizeSeries(string[] names, int size_target, int season, int episode, int length)
    {
        var matches = MatchBuilder(names);
        var result_item = new List<IndexerSearchResultItem>();
        var count = size_target;
        foreach (var match in matches)
        {
            var size = Utils.RandomByteSize(size_target, count);

            result_item.Add(new IndexerSearchResultItem
            {
                name = match,
                size = size,
                files = 1,
                indexer = "",
                link = "",
                type = IndexerResultType.JACKETT,
                torrent_peers = 10,
                torrent_seeders = 20,
            });
            if (count > 1)
            {
                count--;
            }
            else if (count == 0)
            {
                count = size_target;
            }
        }
        var search_job_results = new IndexerSearchJobResults
        {
            jackett_indexer_results = new List<List<JackettIndexerDetailsResults>> { }, // mainly for stat tracking safe to skip
            hydra_indexer_results = new List<List<NzbHydraIndexerDetailsResults>> { }, // --- above --
            results = result_item,
        };

        var request = new IngestSonarrRequest
        {
            length = size_target,
            release = $"Season.{season}.264.Webdl",
            rid = 123,
            tvdb_id = 123,
            season = season
        };
        var request_job = new RequestJob
        {
            ingest = new Ingest
            {
                hash = Utils.RandomHexadecimal(40),
                sonarr = new IngestSonarr
                {
                    series = new SonarrResponseSeries
                    {
                        id = 123,
                        title = "Generic Test Show Name",
                        sort_title = "Generic Test Show Name",
                        seasons = new[] {
                            new SonarrResponseSeason {
                                season_number = season,
                                statistics = new SonarrResponseSeasonStatistics {
                                    total_episode_count = length
                                }}
                        },
                    },
                    request = request
                },
                status = new IngestStatus
                {

                    completed = false,
                    download_speed = 0,
                    eta = 0,
                    name = "Generic Test Show Name",
                    progress = 0,
                    save_path = "/",
                    size = size_target,
                    state = TorrentState.DOWNLOADING
                }
            }
        };
        var search_job = new IndexerSearchJob
        {
            finished = false,
            target = new IndexerSearchJobTarget
            {
                season = season,
                episode = episode
            },
            indexer_search_job = search_job_results
        };
        var matched = Matching.Match(request_job, search_job);
        return matched;
    }
    public static DecisionsDataSort Sort(string x, string y, int x_size = 1000, int y_size = 1000)
    {
        return new DecisionsDataSort
        {
            x = Match(new[] { "Generic Test Show Name " + x }, x_size).results[0],
            y = Match(new[] { "Generic Test Show Name " + y }, y_size).results[0],
        };
    }
}
