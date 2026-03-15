using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Cortexerr.Core.Configuration;
using Cortexerr.Core.Errors;
using Cortexerr.Core.Logging;
using Cortexerr.Core.Utilities;

namespace Cortexerr.Core.Downloaders;

public record RdtClientTorrentResponse
{
    public long? added_on { get; init; }
    public long? amount_left { get; init; }
    public bool? auto_tmm { get; init; }
    public double? availability { get; init; }
    public string? category { get; init; }
    public long? completed { get; init; }
    public long? completion_on { get; init; }
    public string? content_path { get; init; }
    public long? dl_limit { get; init; }
    public long? dlspeed { get; init; }
    public long? downloaded { get; init; }
    public long? downloaded_session { get; init; }
    public long? eta { get; init; }
    public bool? f_l_piece_prio { get; init; }
    public bool? force_start { get; init; }
    public string? hash { get; init; }
    public bool? isPrivate { get; init; }
    public long? last_activity { get; init; }
    public string? magnet_uri { get; init; }
    public double? max_ratio { get; init; }
    public long? max_seeding_time { get; init; }
    public string? name { get; init; }
    public long? num_complete { get; init; }
    public long? num_incomplete { get; init; }
    public long? num_leechs { get; init; }
    public long? num_seeds { get; init; }
    public long? priority { get; init; }
    public float? progress { get; init; }
    public double? ratio { get; init; }
    public double? ratio_limit { get; init; }
    public string? save_path { get; init; }
    public long? seeding_time { get; init; }
    public long? seeding_time_limit { get; init; }
    public long? seen_complete { get; init; }
    public bool? seq_dl { get; init; }
    public long? size { get; init; }
    [JsonConverter(typeof(TorrentStateConverter))]
    public TorrentState state { get; init; }
    public bool? super_seeding { get; init; }
    public string? tags { get; init; }
    public long? time_active { get; init; }
    public long? total_size { get; init; }
    public string? tracker { get; init; }
    public long? up_limit { get; init; }
    public long? uploaded { get; init; }
    public long? uploaded_session { get; init; }
    public long? upspeed { get; init; }
}


public sealed class RdtClient(string magnet)
{
    private static readonly CookieContainer _cookie_container = new();
    private static readonly HttpClientHandler _handler = new()
    { CookieContainer = _cookie_container };
    private static readonly HttpClient _client = new(_handler)
    { BaseAddress = Config.ARGS.rdtclient, };
    private bool _proxied { get; set; }
    private int _retry_instance { get; set; } = 0;
    public string magnet { get; } = magnet;
    public string? hash { get; private set; }


    private HandleResponse<object> Hash()
    {
        if (string.IsNullOrEmpty(magnet))
            return Response.Error(ErrorCode.INVALID_INPUT, "(RdtClient|Hash) invalid magnet or url");
        var match = Regex.Match(magnet, @"xt=urn:[^:]+:([^&\s]+)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            hash = match.Groups[1].Value;
            return Response.Success();
        }
        if (magnet.Contains("jackett"))
        {
            _proxied = true;
            return Response.Success();
        }
        return Response.Error(ErrorCode.INVALID_INPUT, "(RdtClient|Hash) not a valid link");
    }

    private HandleResponse<string> Name()
    {
        if (string.IsNullOrEmpty(magnet))
            return Response.Error<string>(ErrorCode.INVALID_INPUT, "(RdtClient|Name) invalid magnet or url");
        var match = Regex.Match(magnet, @"[?&]file=([^&\s]+)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return Response.Success(match.Groups[1].Value);
        }
        return Response.Error<string>(ErrorCode.INVALID_INPUT, "(RdtClient|Name) no valid name");
    }

    async private Task<HandleResponse<object>> Authenticate()
    {
        const string endpoint = "api/v2/auth/login";
        if (Env.RDTCLIENT_USERNAME == null || Env.RDTCLIENT_PASSWORD == null)
            return Response.Error(ErrorCode.DISABLED, "(RdtClient|Authenticate) missing credentials");

        Logger.Log.Debug($"(RdtClient|Authenticate) url: {Config.ARGS.rdtclient}{endpoint}");
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("username", Env.RDTCLIENT_USERNAME),
            new KeyValuePair<string, string>("password", Env.RDTCLIENT_PASSWORD)
        });

        var handled_response = await Error.HandleAsync(async () =>
        {
            return await _client.PostAsync(endpoint, content);
        });
        if (handled_response.error != null) return Response.Error(handled_response.error.code, handled_response.error.message);
        if (handled_response.data?.StatusCode == HttpStatusCode.Forbidden || handled_response.data == null)
        {
            return Response.Error(ErrorCode.INVALID_INPUT, "RdtClient username / password could be invalid");
        }
        if (_client.BaseAddress != null)
        {
            var sid = _cookie_container.GetCookies(_client.BaseAddress)["SID"];
            Logger.Log.Debug($"(RdtClient|Authenticate) sid: {sid}");
            if (sid != null)
            {
                return Response.Success();
            }
        }
        return Response.Error(ErrorCode.REJECTED, "(RdtClient|Authenticate) Request rejected");
    }

    async private Task<HandleResponse<T>> RequestGet<T>(string endpoint)
    {
        Logger.Log.Debug($"(RdtClient|RequestGet) url: {Config.ARGS.rdtclient}{endpoint}");
        var handled_response = await Error.HandleAsync<HttpResponseMessage>(async () =>
        {
            return await _client.GetAsync(endpoint);
        });
        if (handled_response.error != null) return Response.Error<T>(handled_response.error.code, handled_response.error.message);
        if (handled_response.data == null) return Response.Error<T>(ErrorCode.UNEXPECTED_ERROR, "(RdtClient|RequestGet) Failed to get url");
        if (handled_response.data.StatusCode == HttpStatusCode.Forbidden)
        {
            var auth = await Authenticate();
            if (auth.error != null) return Response.Error<T>(auth.error.code, auth.error.message);
            handled_response = await Error.HandleAsync<HttpResponseMessage>(async () =>
            {
                return await _client.GetAsync(endpoint);
            });
            if (handled_response.error != null) return Response.Error<T>(handled_response.error.code, handled_response.error.message);
            if (handled_response.data == null) return Response.Error<T>(ErrorCode.UNEXPECTED_ERROR, "(RdtClient|RequestGet) Failed to get url");
            if (handled_response.data.StatusCode == HttpStatusCode.Forbidden)
            {
                return Response.Error<T>(ErrorCode.DISABLED, "(RdtClient|RequestGet) Failed authentication");
            }
        }
        var handled_json = await Error.HandleAsync<T?>(async () =>
        {
            var response = await handled_response.data.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(response);
        });
        if (handled_json.error != null) return Response.Error<T>(handled_json.error.code, handled_json.error.message);
        if (handled_json.data == null) return Response.Error<T>(ErrorCode.UNEXPECTED_ERROR, "(RdtClient|RequestGet) Could not parse json data");
        return Response.Success(handled_json.data);
    }

    async private Task<HandleResponse<HttpResponseMessage>> RequestPost(string endpoint, HttpContent data)
    {
        Logger.Log.Debug($"(RdtClient|RequestPost) url: {Config.ARGS.rdtclient}{endpoint}");
        var handled_response = await Error.HandleAsync<HttpResponseMessage>(async () =>
        {
            return await _client.PostAsync(endpoint, data);
        });
        if (handled_response.error != null)
            return Response.Error<HttpResponseMessage>(handled_response.error.code, handled_response.error.message);
        if (handled_response.data == null)
            return Response.Error<HttpResponseMessage>(ErrorCode.UNEXPECTED_ERROR, "(RdtClient|RequestPost) Failed to get url");
        if (handled_response.data.StatusCode == HttpStatusCode.Forbidden)
        {
            var auth = await Authenticate();
            if (auth.error != null) return Response.Error<HttpResponseMessage>(auth.error.code, auth.error.message);
            handled_response = await Error.HandleAsync<HttpResponseMessage>(async () =>
            {
                return await _client.PostAsync(endpoint, data);
            });
            if (handled_response.error != null)
                return Response.Error<HttpResponseMessage>(handled_response.error.code, handled_response.error.message);
            if (handled_response.data == null)
                return Response.Error<HttpResponseMessage>(ErrorCode.UNEXPECTED_ERROR, "(RdtClient|RequestPost) Failed to get url");
            if (handled_response.data.StatusCode == HttpStatusCode.Forbidden)
                return Response.Error<HttpResponseMessage>(ErrorCode.DISABLED, "(RdtClient|RequestPost) Failed authentication");
        }
        return Response.Success(handled_response.data);
    }

    async public Task<HandleResponse<RdtClientTorrentResponse>> Torrent(bool polling = true)
    {
        // will try to find torrent 3 times until failure since there there can be a delay after adding until it shows up
        if (string.IsNullOrEmpty(hash))
            return Response.Error<RdtClientTorrentResponse>(ErrorCode.NOT_FOUND, "(RdtClient|Torrent) Invalid hash");
        var endpoint = $"api/v2/torrents/info?hashes={hash}";
        var response = await RequestGet<RdtClientTorrentResponse[]>(endpoint);
        if (response.data != null)
        {
            foreach (var torrent in response.data)
            {
                if (torrent.hash != hash) continue;
                if (polling)
                {
                    if (torrent.progress != null)
                    {
                        var progress = torrent.progress * 100;
                        if (progress <= 5 || progress >= 95)
                        { await Task.Delay(5000); }
                        else
                        { await Task.Delay(20000); }
                    }
                    else
                    {
                        return Response.Error<RdtClientTorrentResponse>(ErrorCode.UNEXPECTED_ERROR, "(RdtClient|Torrent) Missing torrent progress");
                    }
                }
                return Response.Success(torrent);
            }
        }
        if (_retry_instance < Config.ARGS.api_retry_timeout)
        {
            _retry_instance++;
            var delay = 1 << _retry_instance;
            await Task.Delay(delay * 1000);
            return await Torrent(false);
        }
        if (_retry_instance == Config.ARGS.api_retry_timeout)
        {
            return Response.Error<RdtClientTorrentResponse>(ErrorCode.TIMEOUT,
                    $"(RdtClient|Torrent) Retried endpoints ({_retry_instance + 1}) times, ({1 << _retry_instance}) seconds of delay");
        }
        return Response.Error<RdtClientTorrentResponse>(ErrorCode.NOT_FOUND, "(RdtClient|Torrent) Could not find torrent");
    }

    async public Task<HandleResponse<object>> Add()
    {
        var endpoint = "api/v2/torrents/add";
        var hash_response = Hash();
        if (hash_response.error != null) return hash_response;

        // urls
        if (_proxied)
        {
            var torrent = await _client.GetByteArrayAsync(magnet);
            var byte_array = new ByteArrayContent(torrent);
            byte_array.Headers.ContentType = new MediaTypeHeaderValue("application/x-bittorrent");
            using var content = new MultipartFormDataContent();

            var name = Name();
            string category;
            if (name.data != null)
            {
                content.Add(byte_array, "torrents", $"{name.data}.torrent");
                category = $"{name.data}-{Utils.RandomHexadecimal(8)}";
            }
            else
            {
                content.Add(byte_array, "torrents", "torrent.torrent");
                category = Utils.RandomHexadecimal(32);
            }
            content.Add(new StringContent(category), "category");
            var response = await RequestPost(endpoint, content);
            if (response.error != null) return Response.Error(response.error.code, response.error.message);
            if (response.data == null) return Response.Error(ErrorCode.UNEXPECTED_ERROR, "(RdtClient|Add) Data and error null");
            if (response.data.IsSuccessStatusCode)
            {
                var proxied_torrent_endpoint = $"/api/v2/torrents/info?category={category}";
                var torrent_response = await RequestGet<RdtClientTorrentResponse[]>(proxied_torrent_endpoint);
                if (torrent_response.error != null)
                    return Response.Error(torrent_response.error.code, torrent_response.error.message);
                if (!string.IsNullOrEmpty(torrent_response.data?[0].hash))
                {
                    hash = torrent_response.data[0].hash;
                    return Response.Success();
                }
                return Response.Error(ErrorCode.NOT_FOUND, "(RdtClient|Add) could not get proxied torrent");
            }
        }
        // magnet links
        else
        {
            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("urls", magnet),
        });
            var response = await RequestPost(endpoint, content);
            if (response.error != null) return Response.Error(response.error.code, response.error.message);
            if (response.data == null) return Response.Error(ErrorCode.UNEXPECTED_ERROR, "(RdtClient|Add) Data and error null");
            if (response.data.IsSuccessStatusCode)
            {
                return Response.Success();
            }
        }
        return Response.Error(ErrorCode.REJECTED, "(RdtClient|Add) Failed to add");
    }

    async public Task<HandleResponse<object>> Delete()
    {
        if (string.IsNullOrEmpty(hash))
            return Response.Error(ErrorCode.NOT_FOUND, "(RdtClient|Torrent) Invalid hash");
        var endpoint = "api/v2/torrents/delete";

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("hashes", hash),
        });
        var response = await RequestPost(endpoint, content);
        if (response.error != null) return Response.Error(response.error.code, response.error.message);
        if (response.data == null) return Response.Error(ErrorCode.UNEXPECTED_ERROR, "(RdtClient|Add) Data and error null");
        if (response.data.IsSuccessStatusCode)
        {
            return Response.Success();
        }
        return Response.Error(ErrorCode.REJECTED, "(RdtClient|Add) Failed to add");
    }
}
