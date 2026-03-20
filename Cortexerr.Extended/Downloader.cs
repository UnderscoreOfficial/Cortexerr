using Cortexerr.Extended.DataStructures;

namespace Cortexerr.Extended.Downloader;

public static class Downloader
{
  async public static void Download(RequestJob request_job)
  {
    var ingest = request_job.ingest;
    if (ingest.sonarr != null)
    {
      var sonarr = ingest.sonarr;

    }
    else if (ingest.radarr != null)
    {
      var radarr = ingest.radarr;

    }
  }
}
