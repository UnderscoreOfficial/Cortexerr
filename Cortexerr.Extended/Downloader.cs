using Cortexerr.Core.Configuration;
using Cortexerr.Core.Downloaders;
using Cortexerr.Core.Errors;
using Cortexerr.Core.Ingest;
using Cortexerr.Extended.DataStructures;
using Cortexerr.Extended.Indexer;

namespace Cortexerr.Extended.Downloader;

public sealed record DownloadJobResultsInstance
{
  public Sabnzbd? sabnzbd { get; init; }
  public RdtClient? rdtclient { get; init; }
}
public sealed record DownloadJobResults
{
  public required IndexerSearchResultItem item { get; init; }
  public required HandleResponse<DownloadJobResultsInstance> instance { get; init; }
  public TorrentState? rdtclient_state { get; init; }
  public NzbState? sabnzbd_state { get; init; }
}
public sealed record DownloadJob
{
  public required List<DownloadJobResults> results { get; init; }
}

public static class Downloader
{
  private static DownloadJobResults BuildDownloadJobResults(
      DownloadJob download_job,
      IndexerSearchResultItem item,
      HandleResponse<DownloadJobResultsInstance> instance,
      TorrentState? rdtclient_state = null,
      NzbState? sabnzbd_state = null)
  {
    var download_job_results = new DownloadJobResults
    {
      item = item,
      instance = instance,
      rdtclient_state = rdtclient_state,
      sabnzbd_state = sabnzbd_state
    };
    download_job.results.Add(download_job_results);
    return download_job_results;
  }

  async private static Task<HandleResponse<DownloadJobResults>> SabnzbdDownload(
      DownloadJob download_job,
      IndexerSearchResultItem item)
  {
    var sabnzbd = new Sabnzbd(item.link);
    var response = await sabnzbd.Add();
    string error_message;
    if (response.error != null)
    {
      BuildDownloadJobResults(download_job, item,
          Response.Error<DownloadJobResultsInstance>(response.error.code, response.error.message));
      return Response.Error<DownloadJobResults>(response.error.code, response.error.message);
    }
    if (response.data != null)
    {
      NzbState? last_state = null;
      DateTime? max_time = null;

      var size = 1;
      var timeout = 5d;
      var validated_size = false;

      while (true)
      {
        var status = await sabnzbd.Nzb();
        if (status.error != null)
        {
          BuildDownloadJobResults(download_job, item,
              Response.Error<DownloadJobResultsInstance>(status.error.code, status.error.message));
          return Response.Error<DownloadJobResults>(status.error.code, status.error.message);
        }

        if (status.data == null)
        {
          error_message = "(Downloader|SabnzbdDownload) Missing data and no error";
          BuildDownloadJobResults(download_job, item,
              Response.Error<DownloadJobResultsInstance>(ErrorCode.REJECTED, error_message));
          return Response.Error<DownloadJobResults>(ErrorCode.UNEXPECTED_ERROR, error_message);
        }
        var parsed_size = status.data.queue?.size?.Split(" ");

        if (!validated_size)
        {
          if (parsed_size?[1] == "GB")
          {
            if (int.TryParse(parsed_size[0], out int value)) size = value;
          }
          timeout = Config.ARGS.download_timeout_factor * Math.Sqrt(size);
          validated_size = true;
        }

        var state = status.data.status;
        switch (state)
        {
          case NzbState.COMPLETED:
            var results = BuildDownloadJobResults(download_job, item,
                Response.Success(new DownloadJobResultsInstance { sabnzbd = sabnzbd }), null, state);
            return Response.Success(results);
          case NzbState.FAILED:
            await sabnzbd.Delete();
            error_message = $"(Downloader|SabnzbdDownload) Failed to download ({item.link})";
            BuildDownloadJobResults(download_job, item,
                Response.Error<DownloadJobResultsInstance>(ErrorCode.REJECTED, error_message), null, state);
            return Response.Error<DownloadJobResults>(ErrorCode.REJECTED, error_message);
          default:
            var timed_out = false;
            if (last_state == state)
            {
              if (DateTime.Now >= max_time) timed_out = true;
            }
            else
            {
              max_time = DateTime.Now.AddMinutes(timeout);
            }
            if (timed_out)
            {
              await sabnzbd.Delete();
              error_message = $"(Downloader|SabnzbdDownload) Timed out ({state.ToString()})";
              BuildDownloadJobResults(download_job, item,
                  Response.Error<DownloadJobResultsInstance>(ErrorCode.TIMEOUT, error_message), null, state);
              return Response.Error<DownloadJobResults>(ErrorCode.TIMEOUT, error_message);
            }
            break;
        }
        last_state = status.data.status;
      }
    }
    error_message = "(Downloader|SabnzbdDownload) Unknown error state";
    BuildDownloadJobResults(download_job, item,
        Response.Error<DownloadJobResultsInstance>(ErrorCode.TIMEOUT, error_message));
    return Response.Error<DownloadJobResults>(ErrorCode.UNEXPECTED_ERROR, error_message);
  }

  async private static Task<HandleResponse<DownloadJobResults>> RdtClientDownload(
      DownloadJob download_job,
      IndexerSearchResultItem item)
  {
    var rdtclient = new RdtClient(item.link);
    var response = await rdtclient.Add();
    string error_message;
    if (response.error != null)
    {
      BuildDownloadJobResults(download_job, item,
          Response.Error<DownloadJobResultsInstance>(response.error.code, response.error.message));
      return Response.Error<DownloadJobResults>(response.error.code, response.error.message);
    }
    if (response.data != null)
    {
      TorrentState? last_state = null;
      DateTime? max_time = null;

      var size = 1;
      var timeout = 5d;
      var validated_size = false;

      while (true)
      {
        var status = await rdtclient.Torrent();
        if (status.error != null)
        {
          BuildDownloadJobResults(download_job, item,
              Response.Error<DownloadJobResultsInstance>(status.error.code, status.error.message));
          return Response.Error<DownloadJobResults>(status.error.code, status.error.message);
        }
        if (status.data == null)
        {
          error_message = "(Downloader|RdtClientDownload) Missing data and no error";
          BuildDownloadJobResults(download_job, item,
              Response.Error<DownloadJobResultsInstance>(ErrorCode.UNEXPECTED_ERROR, error_message));
          return Response.Error<DownloadJobResults>(ErrorCode.UNEXPECTED_ERROR, error_message);
        }

        var BYTE_MULTIPLIER = 1_000_000_000;
        var parsed_size = status.data.size / BYTE_MULTIPLIER;
        if (!validated_size)
        {
          if (parsed_size != null)
          {
            if (parsed_size <= int.MaxValue)
            {
              size = (int)parsed_size;
            }
            else
            {
              size = int.MaxValue;
            }
          }
          timeout = Config.ARGS.download_timeout_factor * Math.Sqrt(size);
          validated_size = true;
        }

        var state = status.data.state;
        switch (state)
        {
          case TorrentState.STALLED_UP:
          case TorrentState.UPLOADING:
            var results = BuildDownloadJobResults(download_job, item,
                Response.Success(new DownloadJobResultsInstance { rdtclient = rdtclient }), state);
            return Response.Success(results);
          case TorrentState.ERROR:
          case TorrentState.MISSING_FILES:
            await rdtclient.Delete();
            error_message = $"(Downloader|RdtClientDownload) Failed to download ({item.link})";
            BuildDownloadJobResults(download_job, item,
                Response.Error<DownloadJobResultsInstance>(ErrorCode.REJECTED, error_message), state);
            return Response.Error<DownloadJobResults>(ErrorCode.REJECTED, error_message);
          default:
            var timed_out = false;
            if (last_state == state)
            {
              if (DateTime.Now >= max_time) timed_out = true;
            }
            else
            {
              max_time = DateTime.Now.AddMinutes(timeout);
            }
            if (timed_out)
            {
              await rdtclient.Delete();
              error_message = $"(Downloader|RdtClientDownload) Timed out ({state.ToString()})";
              BuildDownloadJobResults(download_job, item,
                  Response.Error<DownloadJobResultsInstance>(ErrorCode.TIMEOUT, error_message), state);
              return Response.Error<DownloadJobResults>(ErrorCode.TIMEOUT, error_message);
            }
            break;
        }
        last_state = state;
      }
    }
    error_message = "(Downloader|RdtClientDownload) Unknown error state";
    BuildDownloadJobResults(download_job, item,
        Response.Error<DownloadJobResultsInstance>(ErrorCode.UNEXPECTED_ERROR, error_message));
    return Response.Error<DownloadJobResults>(ErrorCode.UNEXPECTED_ERROR, error_message);
  }

  async public static Task<HandleResponse<DownloadJobResults>> Download(RequestJob request_job, List<IndexerSearchResultItem> indexer_search_result_items)
  {
    if (indexer_search_result_items.Count == 0)
      return Response.Error<DownloadJobResults>(ErrorCode.INVALID_INPUT, "(Downloader|Download) No items to download");
    var download_job = new DownloadJob { results = new List<DownloadJobResults>() };
    foreach (var item in indexer_search_result_items)
    {
      if (item.type == IndexerResultType.NZBHYDRA)
      {
        var sabnzbd = await SabnzbdDownload(download_job, item);
        if (sabnzbd.error?.code == ErrorCode.REJECTED || sabnzbd.error?.code == ErrorCode.TIMEOUT)
        {
          continue;
        }
        request_job.download_jobs.Add(download_job);
        return sabnzbd;
      }
      if (item.type == IndexerResultType.JACKETT)
      {
        var rdtclient = await RdtClientDownload(download_job, item);
        if (rdtclient.error?.code == ErrorCode.REJECTED || rdtclient.error?.code == ErrorCode.TIMEOUT)
        {
          continue;
        }
        request_job.download_jobs.Add(download_job);
        return rdtclient;
      }
    }
    return Response.Error<DownloadJobResults>(ErrorCode.UNEXPECTED_ERROR, "(Downloader|Download) Unknown error state");
  }
}
