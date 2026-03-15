using System.Text;
using System.Web;
using System.Xml;
using Cortexerr.Core.Arrs;
using Cortexerr.Core.Configuration;
using Cortexerr.Core.Errors;
using Cortexerr.Core.Utilities;
using MonoTorrent.BEncoding;

namespace Cortexerr.App.Base;

public static class TorznabRoutes
{
  public static IResult TestSearch()
  {
    var search = $"""
    <?xml version="1.0" encoding="UTF-8"?>
      <rss version="2.0"
           xmlns:torznab="http://torznab.com/schemas/2015/feed"
           xmlns:newznab="http://www.newznab.com/DTD/2010/feeds/attributes/">
        <channel>
          <title>FakeTorznab</title>
          <description>Torznab API results</description>
          <link>{Config.ARGS.host}</link>
          <newznab:response offset="0" total="2" />
          <item>
            <title>Some.Show.S01E01.720p.WEB-DL.x264</title>
            <guid isPermaLink="false">fake-torznab-guid-1</guid>
            <pubDate>Wed, 07 Jan 2026 21:20:00 -0500</pubDate>
            <category>TV > HD</category>
            <description>Fake result for testing.</description>
            <enclosure url="{Config.ARGS.host}download/1.torrent"
                       length="123456789"
                       type="application/x-bittorrent" />
            <newznab:attr name="category" value="5040" />
            <newznab:attr name="category" value="5000" />
            <torznab:attr name="seeders" value="25" />
            <torznab:attr name="leechers" value="3" />
            <torznab:attr name="size" value="123456789" />
          </item>
          <item>
            <title>Some.Show.S01E01.HDTV.x264</title>
            <guid isPermaLink="false">fake-torznab-guid-2</guid>
            <pubDate>Wed, 07 Jan 2026 21:19:00 -0500</pubDate>
            <category>TV > SD</category>
            <description>Fake result for testing.</description>
            <enclosure url="{Config.ARGS.host}download/2.torrent"
                       length="987654321"
                       type="application/x-bittorrent" />
            <newznab:attr name="category" value="5030" />
            <newznab:attr name="category" value="5000" />
            <torznab:attr name="seeders" value="10" />
            <torznab:attr name="leechers" value="1" />
            <torznab:attr name="size" value="987654321" />
          </item>
        </channel>
      </rss>
    """;
    return Results.Content(search, "application/xml", Encoding.UTF8, 200);
  }

  public static IResult TestCapabilities()
  {
    var capabilities = $"""
    <?xml version="1.0" encoding="UTF-8"?>
      <caps>
        <server version="2.0"
                title="CortexerrTorznab"
                strapline="Torznab Cortexerr Capabilities"
                email="cortexerr@example.com"
                url="{Config.ARGS.host}"
                image="{Config.ARGS.host}cortexerr.png" />
        <limits max="100" default="100" />
        <registration available="no" open="no" />
        <searching>
          <search available="no" supportedParams="q" />
          <tv-search available="yes" supportedParams="tvdbid,season,ep" />
          <movie-search available="yes" supportedParams="tmdbid" />
          <audio-search available="no" supportedParams="q" />
          <book-search available="no" supportedParams="q" />
        </searching>
        <categories>
          <category id="5000" name="TV">
            <subcat id="5070" name="Anime" />
          </category>
          <category id="2000" name="Movies">
            <subcat id="2060" name="3D" />
          </category>
        </categories>
      </caps>
    """;
    return Results.Content(capabilities, "application/xml", Encoding.UTF8, 200);
  }

  public static XmlElement BuildTorznabAttribute(XmlDocument xml, string name, string value)
  {
    var attribute = xml.CreateElement("torznab", "attr", "http://torznab.com/schemas/2015/feed");
    attribute.SetAttribute("name", name);
    attribute.SetAttribute("value", value);
    return attribute;
  }

  public static XmlElement BuildItem(
      XmlDocument xml,
      string hash,
      string release_name,
      string generated_link,
      long random_size
    )
  {
    var item = xml.CreateElement("item");

    var title = xml.CreateElement("title");
    title.AppendChild(xml.CreateTextNode(release_name));
    item.AppendChild(title);

    var guid = xml.CreateElement("guid");
    guid.SetAttribute("isPermaLink", "false");
    guid.AppendChild(xml.CreateTextNode($"""
      magnet:?xt=urn:btih:{hash}&amp;dn={release_name}&amp;tr=http%3a%2f%2ftracker.opentrackr.org%3a1337%2fannounce;
    """)
    );
    item.AppendChild(guid);

    var pub_date = xml.CreateElement("pubDate");
    pub_date.AppendChild(xml.CreateTextNode(Utils.RandomDateTime().ToString()));
    item.AppendChild(pub_date);

    var size = xml.CreateElement("size");
    size.AppendChild(xml.CreateTextNode(random_size.ToString()));
    item.AppendChild(size);

    var link = xml.CreateElement("link");
    link.AppendChild(xml.CreateTextNode(generated_link));
    item.AppendChild(link);

    var enclosure = xml.CreateElement("enclosure");
    enclosure.SetAttribute("url", generated_link);
    enclosure.SetAttribute("length", random_size.ToString());
    enclosure.SetAttribute("type", "application/x-bittorrent");
    item.AppendChild(enclosure);

    item.AppendChild(BuildTorznabAttribute(xml, "tag", "freeleech"));
    item.AppendChild(BuildTorznabAttribute(xml, "seeders", "100"));
    item.AppendChild(BuildTorznabAttribute(xml, "leechers", "10"));
    return item;
  }

  public static XmlElement BuildMovieItem(XmlDocument xml, RadarrResponseMovie movie, string release)
  {
    var formated_release = release.Length > 0 ? $".{release}" : "";
    var formatted_name = $"{movie.sort_title?.Replace(" ", ".")}{formated_release}";
    var release_name = $"{formatted_name}.${movie.year}.1080p.WEB-DL.x264";

    string hash;
    do
    {
      hash = Utils.RandomHexadecimal(40);
    }
    while (Requested.ingest.ContainsKey(hash));
    var random_size = Utils.RandomByteSize(50, 10);
    var generated_link_base = "api/download?";
    var link_query = HttpUtility.ParseQueryString(String.Empty);
    link_query.Add("hash", hash);
    link_query.Add("id", movie.id.ToString());
    link_query.Add("tmdbid", movie.tmdb_id.ToString());
    link_query.Add("name", release_name);
    link_query.Add("release", release);
    link_query.Add("length", random_size.ToString());
    var generated_link = $"{generated_link_base}{link_query.ToString()}";

    var item = BuildItem(xml, hash, release_name, generated_link, random_size);
    item.AppendChild(BuildTorznabAttribute(xml, "category", "2000"));
    if (movie.imdb_id != null) item.AppendChild(BuildTorznabAttribute(xml, "imdbid", movie.imdb_id));
    return item;
  }

  public static XmlElement BuildSeriesItem(
      XmlDocument xml,
      SonarrResponseSeries series,
      string release,
      int? season = null,
      int? episode = null
    )
  {
    var formated_release = release.Length > 0 ? $".{release}" : "";
    var formatted_name = $"{series.title_slug?.Replace("-", ".").Replace(" ", ".")}{formated_release}";
    var release_name = $"{formatted_name}.{series.year}.1080p.WEB-DL.x264";

    string hash;
    do
    {
      hash = Utils.RandomHexadecimal(40);
    }
    while (Requested.ingest.ContainsKey(hash));
    var random_size = Utils.RandomByteSize(20);
    if (release.Contains("series", StringComparison.OrdinalIgnoreCase)
        || release.Contains("season", StringComparison.OrdinalIgnoreCase)
        || release.Contains(".s", StringComparison.OrdinalIgnoreCase))
      random_size = Utils.RandomByteSize(100, 20);

    var generated_link_base = "api/download?";
    var link_query = HttpUtility.ParseQueryString(String.Empty);
    link_query.Add("hash", hash);
    link_query.Add("id", series.id.ToString());
    link_query.Add("tvdbid", series.tvdb_id.ToString());
    link_query.Add("name", release_name);
    link_query.Add("release", release);
    link_query.Add("length", random_size.ToString());
    if (season != null) link_query.Add("season", season.ToString());
    if (episode != null) link_query.Add("episode", episode.ToString());
    var generated_link = $"{generated_link_base}{link_query.ToString()}";

    var item = BuildItem(xml, hash, release_name, generated_link, random_size);
    item.AppendChild(BuildTorznabAttribute(xml, "category", "5000"));
    if (series.imdb_id != null) item.AppendChild(BuildTorznabAttribute(xml, "imdbid", series.imdb_id));
    return item;
  }

  public static XmlDocument SearchResultsTemplate()
  {
    var template =
    $"""
    <?xml version="1.0" encoding="UTF-8"?>
    <rss version="2.0"
         xmlns:atom="http://www.w3.org/2005/Atom"
         xmlns:torznab="http://torznab.com/schemas/2015/feed"
         xmlns:newznab="http://www.newznab.com/DTD/2010/feeds/attributes/">
      <channel>
        <title>Torznab</title>
        <atom:link href="{Config.ARGS.host}" rel="self" type="application/rss+xml" />
      </channel>
    </rss>
    """;
    var xml_doc = new XmlDocument();
    xml_doc.LoadXml(template);
    return xml_doc;
  }

  async public static Task<IResult> RadarrMovie(int tmdb_id)
  {
    var movie = await Radarr.Movie(tmdb_id);
    if (movie.error != null)
      return Results.BadRequest($"[{movie.error.code.ToString()}] {movie.error.message}");
    if (movie.data != null)
    {
      var xml = SearchResultsTemplate();
      var channel = xml.SelectSingleNode("/rss/channel");
      if (channel == null) return Results.InternalServerError();
      channel.AppendChild(BuildMovieItem(xml, movie.data, ""));
      var string_writer = new StringWriter();
      xml.Save(string_writer);
      return Results.Content(string_writer.ToString(), "application/xml", Encoding.UTF8, 200);
    }
    return Results.BadRequest($"[{ErrorCode.UNEXPECTED_ERROR.ToString()}] (TorznabRoutes|RadarrMovie) Invalid request");
  }

  async public static Task<IResult> SonarrSeries(int tvdb_id, int? season, int? episode)
  {
    var series = await Sonarr.Series(tvdb_id);
    if (series.error != null)
      return Results.BadRequest($"[{series.error.code.ToString()}] {series.error.message}");
    if (series.data != null)
    {
      var xml = SearchResultsTemplate();
      var channel = xml.SelectSingleNode("/rss/channel");
      if (channel == null) return Results.InternalServerError();
      channel.AppendChild(BuildSeriesItem(xml, series.data, "Complete.Series"));
      channel.AppendChild(BuildSeriesItem(xml, series.data, "Full.Series"));
      channel.AppendChild(BuildSeriesItem(xml, series.data, "Season-1"));
      channel.AppendChild(BuildSeriesItem(xml, series.data, "S01"));
      channel.AppendChild(BuildSeriesItem(xml, series.data, "S01E01"));

      if (season != null)
      {
        channel.AppendChild(BuildSeriesItem(xml, series.data, $"Season-{season}", season));
        for (var i = 1; i <= series.data.statistics?.total_episode_count; i++)
        {
          channel.AppendChild(BuildSeriesItem(xml, series.data,
                $"S{season.ToString()?.PadLeft(2, '0')}E{i.ToString()?.PadLeft(2, '0')}", season));
        }
      }
      if (episode != null)
      {
        channel.AppendChild(BuildSeriesItem(xml, series.data, $"S01E{episode.ToString()?.PadLeft(2, '0')}", null, episode));
        channel.AppendChild(BuildSeriesItem(xml, series.data, $"E{episode.ToString()?.PadLeft(2, '0')}", null, episode));
      }
      if (episode != null && season != null)
      {
        channel.AppendChild(BuildSeriesItem(xml, series.data, $"Season-{season}-{episode}", season, episode));
        channel.AppendChild(BuildSeriesItem(xml, series.data,
              $"S{season.ToString()?.PadLeft(2, '0')}E{episode.ToString()?.PadLeft(2, '0')}", season, episode));
        for (var i = 1; i <= episode; i++)
        {
          channel.AppendChild(BuildSeriesItem(xml, series.data,
                $"S{season.ToString()?.PadLeft(2, '0')}E{i.ToString()?.PadLeft(2, '0')}", season, episode));
        }
      }
      var string_writer = new StringWriter();
      xml.Save(string_writer);
      return Results.Content(string_writer.ToString(), "application/xml", Encoding.UTF8, 200);
    }
    return Results.BadRequest($"[{ErrorCode.UNEXPECTED_ERROR.ToString()}] (TorznabRoutes|SonarrSeries) Invalid request");
  }

  public static IResult IndexDownload(EncodedTorrent torrent)
  {
    var info = new BEncodedDictionary
    {
      ["name"] = new BEncodedString(torrent.name),
      ["hash"] = new BEncodedString(torrent.hash),
      ["id"] = new BEncodedNumber(torrent.id),
      ["release"] = new BEncodedString(torrent.release),
      ["length"] = new BEncodedNumber(torrent.length),
      ["piece length"] = new BEncodedNumber(524288),
      ["pieces"] = new BEncodedString(new byte[20 * 8])
    };
    if (torrent.tvdbid.HasValue) info["tvdbid"] = new BEncodedNumber(torrent.tvdbid.Value);
    if (torrent.tmdbid.HasValue) info["tmdbid"] = new BEncodedNumber(torrent.tmdbid.Value);
    if (torrent.season.HasValue) info["season"] = new BEncodedNumber(torrent.season.Value);
    if (torrent.episode.HasValue) info["episode"] = new BEncodedNumber(torrent.episode.Value);

    var root = new BEncodedDictionary
    {
      ["announce"] = new BEncodedString("http://tracker.opentrackr.org:1337/announce"),
      ["info"] = info
    };
    return Results.Bytes(root.Encode(), contentType: "application/x-bittorrent");
  }

  public static WebApplication MapTorznabRoutes(this WebApplication app)
  {
    app.MapGet("/indexer/api/download", ([AsParameters] EncodedTorrent torrent) =>
    {
      return IndexDownload(torrent);
    });
    app.MapGet("/indexer/api", async (string? apikey, string? t, int? tvdbid, int? tmdbid, int? season, int? ep) =>
    {
      // if (String.IsNullOrEmpty(apikey)) return;
      if (t == "caps")
      {
        return TestCapabilities();
      }
      if (tvdbid.HasValue)
      {
        return await SonarrSeries(tvdbid.Value, season, ep);
      }
      if (tmdbid.HasValue)
      {
        return await RadarrMovie(tmdbid.Value);
      }
      if (t == "tvsearch" || t == "movie")
      {
        return TestSearch();
      }
      return Results.NotFound();
    });

    return app;
  }
}
