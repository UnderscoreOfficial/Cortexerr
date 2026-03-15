using System.Text.Json;
using Cortexerr.Core.Configuration;
using Cortexerr.Core.Errors;
using Cortexerr.Core.Logging;

namespace Cortexerr.Core.Downloaders;

public record SabnzbdStatusResponse
{
    public bool status { get; init; }
    public string[]? nzo_ids { get; init; }
}

public record SabnzbdStageLog
{
    public string? name { get; init; }
    public string[]? actions { get; init; }
}
public record SabnzbdHistoryResponseSlot
{
    public string? action_line { get; init; }
    public string? duplicate_key { get; init; }
    public object? meta { get; init; }
    public string? fail_message { get; init; }
    public bool? loaded { get; init; }
    public string? size { get; init; }
    public string? category { get; init; }
    public string? pp { get; init; }
    public bool? retry { get; init; }
    public string? script { get; init; }
    public string? nzb_name { get; init; }
    public int? download_time { get; init; }
    public string? storage { get; init; }
    public bool? has_rating { get; init; }
    public string? status { get; init; }
    public string? script_line { get; init; }
    public long? completed { get; init; }
    public long? time_added { get; init; }
    public string? nzo_id { get; init; }
    public long? downloaded { get; init; }
    public string? report { get; init; }
    public string? password { get; init; }
    public string? path { get; init; }
    public int? postproc_time { get; init; }
    public string? name { get; init; }
    public string? url { get; init; }
    public string? md5sum { get; init; }
    public bool? archive { get; init; }
    public long? bytes { get; init; }
    public string? url_info { get; init; }
    public SabnzbdStageLog[]? stage_log { get; init; }
}
public record SabnzbdHistoryResponseHistory
{
    public string? total_size { get; init; }
    public string? month_size { get; init; }
    public string? week_size { get; init; }
    public string? day_size { get; init; }
    public SabnzbdHistoryResponseSlot[]? slots { get; init; }
    public int? ppslots { get; init; }
    public int? noofslots { get; init; }
    public long? last_history_update { get; init; }
    public string? version { get; init; }
}
public record SabnzbdHistoryResponse
{
    public SabnzbdHistoryResponseHistory? history { get; init; }
}

public record SabnzbdQueueResponseSlot
{
    public string? status { get; init; }
    public int? index { get; init; }
    public string? password { get; init; }
    public string? avg_age { get; init; }
    public int? time_added { get; init; }
    public string? script { get; init; }
    public string? direct_unpack { get; init; }
    public string? mb { get; init; }
    public string? mbleft { get; init; }
    public string? mbmissing { get; init; }
    public string? size { get; init; }
    public string? sizeleft { get; init; }
    public string? filename { get; init; }
    public string[]? labels { get; init; }
    public string? priority { get; init; }
    public string? cat { get; init; }
    public string? timeleft { get; init; }
    public string? percentage { get; init; }
    public string? nzo_id { get; init; }
    public string? unpackopts { get; init; }
}
public record SabnzbdQueueResponseQueue
{
    public string? status { get; init; }
    public string? speedlimit { get; init; }
    public string? speedlimit_abs { get; init; }
    public bool? paused { get; init; }
    public int? noofslots_total { get; init; }
    public int? noofslots { get; init; }
    public int? limit { get; init; }
    public int? start { get; init; }
    public string? timeleft { get; init; }
    public string? speed { get; init; }
    public string? kbpersec { get; init; }
    public string? size { get; init; }
    public string? sizeleft { get; init; }
    public string? mb { get; init; }
    public string? mbleft { get; init; }
    public SabnzbdQueueResponseSlot[]? slots { get; init; }
    public string? diskspace1 { get; init; }
    public string? diskspace2 { get; init; }
    public string? diskspacetotal1 { get; init; }
    public string? diskspacetotal2 { get; init; }
    public string? diskspace1_norm { get; init; }
    public string? diskspace2_norm { get; init; }
    public string? have_warnings { get; init; }
    public string? pause_int { get; init; }
    public string? left_quota { get; init; }
    public string? version { get; init; }
    public int? finish { get; init; }
    public string? cache_art { get; init; }
    public string? cache_size { get; init; }
    public string? finishaction { get; init; }
    public bool? paused_all { get; init; }
    public string? quota { get; init; }
    public bool? have_quota { get; init; }
}
public record SabnzbdQueueResponse
{
    public SabnzbdQueueResponseQueue? queue { get; init; }
}

public record SabnzbdSlotResponse
{
    public string? status { get; init; }
    public SabnzbdQueueResponseSlot? queue { get; init; }
    public SabnzbdHistoryResponseSlot? history { get; init; }
}

public sealed class Sabnzbd(string nzb_link)
{
    private static readonly HttpClient _client = new()
    {
        BaseAddress = Config.ARGS.sabnzbd
    };
    // when not waiting at least 100ms after adding a new nzb endpoints referencing the newly added nzb 
    // take about 100ms before they can be accessed opted for this as the first fallback over the timeouts
    private DateTime added_timestamp { get; set; }
    private int _retry_instance { get; set; } = 0;
    public string nzb_link { get; } = nzb_link;
    public string? nzo_id { get; private set; }

    async private Task<HandleResponse<T>> Request<T>(string endpoint)
    {
        var api = $"api?output=json&apikey={Env.SABNZBD_API_KEY}&";
        Logger.Log.Debug($"(Sabnzbd|Request) url: {Config.ARGS.sabnzbd}{api}{endpoint}");
        var handled_response = await Error.HandleAsync<T>(async () =>
        {
            var response = await _client.GetStringAsync(api + endpoint);
            var json = JsonSerializer.Deserialize<T>(response);
            if (json == null) throw new Exception();
            return json;
        });
        return handled_response;
    }

    async private Task<HandleResponse<SabnzbdHistoryResponseSlot>> History()
    {
        if (nzo_id == null)
            return Response.Error<SabnzbdHistoryResponseSlot>(ErrorCode.INVALID_STATE, "(Sabnzbd|History) Missing nzo id did you call .Add()");
        var endpoint = $"mode=history&nzo_ids={nzo_id}";
        var request = await Request<SabnzbdHistoryResponse>(endpoint);
        if (request.error != null) return Response.Error<SabnzbdHistoryResponseSlot>(request.error.code, request.error.message);

        var nzb = Error.Handle<SabnzbdHistoryResponseSlot?>(() => request.data?.history?.slots?[0]);
        if (nzb.error != null) return Response.Error<SabnzbdHistoryResponseSlot>(ErrorCode.NOT_FOUND, nzb.error.message);
        if (nzb.data != null)
        {
            return Response.Success(nzb.data);
        }
        return Response.Error<SabnzbdHistoryResponseSlot>(ErrorCode.NOT_FOUND, "(Sabnzbd|History) item not found");
    }

    async private Task<HandleResponse<SabnzbdQueueResponseSlot>> Queue()
    {
        if (nzo_id == null)
            return Response.Error<SabnzbdQueueResponseSlot>(ErrorCode.INVALID_STATE, "(Sabnzbd|Queue) Missing nzo id did you call .Add()");
        var endpoint = $"mode=queue&nzo_ids={nzo_id}";
        var request = await Request<SabnzbdQueueResponse>(endpoint);
        if (request.error != null) return Response.Error<SabnzbdQueueResponseSlot>(request.error.code, request.error.message);

        var nzb = Error.Handle<SabnzbdQueueResponseSlot?>(() => request.data?.queue?.slots?[0]);
        if (nzb.error != null) return Response.Error<SabnzbdQueueResponseSlot>(ErrorCode.NOT_FOUND, nzb.error.message);
        if (nzb.data != null)
        {
            return Response.Success(nzb.data);
        }
        return Response.Error<SabnzbdQueueResponseSlot>(ErrorCode.NOT_FOUND, "(Sabnzbd|Queue) item not found");
    }

    async public Task<HandleResponse<SabnzbdSlotResponse>> Nzb(bool polling = true)
    {
        if (nzo_id == null)
            return Response.Error<SabnzbdSlotResponse>(ErrorCode.INVALID_STATE, "(Sabnzbd|Nzb) Missing nzo id did you call .Add()");
        if ((DateTime.UtcNow - added_timestamp).TotalMilliseconds < 200)
            await Task.Delay(200);
        var queue = await Queue();
        HandleResponse<SabnzbdHistoryResponseSlot>? history = null;

        if (queue.error?.code == ErrorCode.NOT_FOUND)
        {
            history = await History();
            if (history.data != null)
            {
                _retry_instance = 0;
                return
                    Response.Success(new SabnzbdSlotResponse() { status = history.data.status, history = history.data, queue = null });
            }
        }

        if (history?.error != null || queue.error != null)
        {
            if (_retry_instance < Config.ARGS.api_retry_timeout)
            {
                _retry_instance++;
                var delay = 1 << _retry_instance;
                await Task.Delay(delay * 1000);
                return await Nzb(false);
            }
            else if (_retry_instance == Config.ARGS.api_retry_timeout)
            {
                var previous_retry_count = _retry_instance;
                _retry_instance = 0;
                if (history?.error != null)
                {
                    return Response.Error<SabnzbdSlotResponse>(ErrorCode.TIMEOUT, history.error.message);
                }
                else if (queue.error != null)
                {
                    return Response.Error<SabnzbdSlotResponse>(ErrorCode.TIMEOUT, queue.error.message);
                }
                else
                {
                    return
                        Response.Error<SabnzbdSlotResponse>(ErrorCode.TIMEOUT,
                            $"(Sabnzbd|Nzb) Retried endpoints ({previous_retry_count + 1}) times, ({1 << _retry_instance}) seconds of delay"
                        );
                }
            }
        }
        if (polling && queue.data != null)
        {
            if (int.TryParse(queue.data.percentage, out int nzb_percentage))
            {
                if (nzb_percentage <= 5 || nzb_percentage >= 95)
                { await Task.Delay(5000); }
                else
                { await Task.Delay(20000); }
            }
            else
            {
                return Response.Error<SabnzbdSlotResponse>(ErrorCode.UNEXPECTED_ERROR,
                        "(Sabnzbd|Nzb) Could not convert (string) nzb.percentage to (int)");
            }
        }
        if (queue.data != null)
        {
            _retry_instance = 0;
            return
                Response.Success(new SabnzbdSlotResponse() { status = queue.data.status, history = null, queue = queue.data });
        }
        return Response.Error<SabnzbdSlotResponse>(ErrorCode.NOT_FOUND, "(Sabnzbd|Nzb) nzb could not be found");
    }

    async public Task<HandleResponse<SabnzbdStatusResponse>> Add()
    {
        var endpoint = $"mode=addurl&name={Uri.EscapeDataString(nzb_link)}&nzbname=&script=Default&priority=-100&pp=-1";
        var request = await Request<SabnzbdStatusResponse>(endpoint);
        if (request.error != null) return request;
        if (request.data?.status == false)
            return Response.Error<SabnzbdStatusResponse>(ErrorCode.INVALID_INPUT, "(Sabnzbd|Add) Invalid nzb link");
        var _nzo_id = request.data?.nzo_ids?[0];
        if (_nzo_id != null)
        {
            nzo_id = _nzo_id;
            added_timestamp = DateTime.UtcNow;
            return request;
        }
        return Response.Error<SabnzbdStatusResponse>(ErrorCode.NOT_FOUND, "(Sabnzbd|Add) No nzo id returned after adding");
    }
    async public Task<HandleResponse<SabnzbdStatusResponse>> Delete(bool delete_file = true)
    {
        if (nzo_id == null)
            return Response.Error<SabnzbdStatusResponse>(ErrorCode.INVALID_STATE, "(Sabnzbd|Delete) Invalid nzo id did you call .Add()");
        if ((DateTime.UtcNow - added_timestamp).TotalMilliseconds < 200)
            await Task.Delay(200);
        var endpoint = $"mode=queue&name=delete&value={nzo_id}";
        if (delete_file) endpoint += "&del_files=1";
        var request = await Request<SabnzbdStatusResponse>(endpoint);
        if (request.error != null) return request;
        if (request.data == null)
            return Response.Error<SabnzbdStatusResponse>(ErrorCode.UNEXPECTED_ERROR, "(Sabnzbd|Delete) Missing data without any error");
        if (request.data.status)
        {
            return request;
        }
        else
        {
            endpoint = $"mode=history&name=delete&value={nzo_id}";
            if (delete_file) endpoint += "&del_files=1";
            request = await Request<SabnzbdStatusResponse>(endpoint);
            if (request.error != null) return request;
            if (request.data == null)
                return Response.Error<SabnzbdStatusResponse>(ErrorCode.UNEXPECTED_ERROR, "(Sabnzbd|Delete) Missing data without any error");
            return request;
        }
    }
}
