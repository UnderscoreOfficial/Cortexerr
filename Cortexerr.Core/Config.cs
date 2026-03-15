namespace Cortexerr.Core.Configuration;

using DotNetEnv = DotNetEnv.Env;
using System.Text.Json;
using System.CommandLine;
using System.Net.Sockets;
using System.Net;
using System.Text.Json.Serialization;
using Cortexerr.Core.Logging;

// this is the only file that should ever have a hard crash and throw errors
//
// const and readonly are mixed as take Const are real compile time const however
//  many other values are runtime const only ever set within this file never to be changed

/// <summary>
/// Compile time constants.
/// </summary>
public static class Const
{
    public const string CONFIG_FILE = "config.json";
    public const string DEFAULT_ADDRESS = "http://127.0.0.1";

    public const int HOST_PORT = 8989;

    public const int SONARR_PORT = 8989;
    public const int RADARR_PORT = 7878;

    public const int JACKETT_PORT = 9117;
    public const int HYDRA_PORT = 5076;

    public const int RDTCLIENT_PORT = 6500;
    public const int SABNZBD_PORT = 8080;
}

// nothing can be null must always have a base value
// non initialized args must be set in Config.DefaultArgs

/// <summary>
/// config.json parsed data structure.
/// </summary>
public record ConfigArgs(
        Uri host, // no default tries to get local ip to use before asigning fallback default
        Uri sonarr,
        Uri radarr,
        Uri jackett,
        Uri hydra,
        Uri rdtclient,
        Uri sabnzbd,
        string[] release_groups, // defined as an array of groups eg. [group1, group2]  
        string sonarr_download_path = "/sonarr",
        string radarr_download_path = "/radarr",
        bool rss_sync = false, // checks for new releases only for sonarr
        int rss_sync_interval = 60,
        int api_retry_timeout = 3, // used for apis that data is not avaliable instantly timeout is 2^api_retry_timeout starts at 2^1
                                   // will call endpoints n+1 times where +1 is a retry without any time out only getting the error messages 
        bool tv_anime = false, // can increase discovering of anime by enabling its category for tv searches
        bool tv_sports = false, // can increase discovering of sports by enabling its category for tv searches
        bool movie_foreign = false, // can increase discovering of foreign movies by enabling its category for movie searches
        bool movie_3D = false // can increase discovering of 3D movies by enabling its category for movie searches
    );

/// <summary>
/// Environment varibles, .env file is if debug mode is true.
/// <list type="bullet">
/// <item>Env.Initialize() - set and validate enviroment variables</item>
/// </list>
/// </summary>
public static class Env
{
    public static string? SONARR_API_KEY { get; private set; }
    public static string? RADARR_API_KEY { get; private set; }

    public static string? JACKETT_API_KEY { get; private set; }
    public static string? HYDRA_API_KEY { get; private set; }

    public static string? SABNZBD_API_KEY { get; private set; }
    public static string? RDTCLIENT_USERNAME { get; private set; }
    public static string? RDTCLIENT_PASSWORD { get; private set; }

    private static void Validate()
    {
        var errors = new List<string>();
        if (string.IsNullOrEmpty(SONARR_API_KEY) && string.IsNullOrEmpty(RADARR_API_KEY))
        {
            errors.Add("At least (Sonarr / Radarr) must include an api key, at least one is required!");
        }
        if (string.IsNullOrEmpty(HYDRA_API_KEY) && string.IsNullOrEmpty(JACKETT_API_KEY))
        {
            errors.Add("At least (Jackett / Hydra) must include an api key, at least one is required!");
        }
        if (string.IsNullOrEmpty(SABNZBD_API_KEY) && (string.IsNullOrEmpty(RDTCLIENT_USERNAME) || string.IsNullOrEmpty(RDTCLIENT_PASSWORD)))
        {
            errors.Add("At least (Sabnzbd / RdtClient) must include an api key / credentials, at least one is required!");
        }
        // if (string.IsNullOrEmpty(SABNZBD_API_KEY) && string.IsNullOrEmpty(DECYPHARR_API_KEY))
        // {
        //     errors.Add("At least (Sabnzbd / Decypharr) must include an api key / credentials, at least one is required!");
        // }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(string.Join(" | ", errors));
        }
    }

    /// <summary>
    /// Initializes Env must be ran before accessing values.
    /// </summary>
    public static void Initialize()
    {
        try
        {
            var env_path = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (File.Exists(env_path))
            {
                DotNetEnv.Load(env_path);
            }
        }
        // ignore error only for dev .env file
        catch { }
        SONARR_API_KEY = Environment.GetEnvironmentVariable("SONARR_API_KEY")?.Trim();
        RADARR_API_KEY = Environment.GetEnvironmentVariable("RADARR_API_KEY")?.Trim();

        JACKETT_API_KEY = Environment.GetEnvironmentVariable("JACKETT_API_KEY")?.Trim();
        HYDRA_API_KEY = Environment.GetEnvironmentVariable("HYDRA_API_KEY")?.Trim();

        SABNZBD_API_KEY = Environment.GetEnvironmentVariable("SABNZBD_API_KEY")?.Trim();
        RDTCLIENT_USERNAME = Environment.GetEnvironmentVariable("RDTCLIENT_USERNAME")?.Trim();
        RDTCLIENT_PASSWORD = Environment.GetEnvironmentVariable("RDTCLIENT_PASSWORD")?.Trim();

        Validate();
    }
}

/// <summary>
/// Mostly Internal CLI arg parsing.
/// <list type="bullet">
/// <item>Arg.CONFIG_PATH - path to config.json</item>
/// <item>Arg.DEBUG - debug mode state</item>
/// <item>Arg.Initialize() - set and validate cli args</item>
/// </list>
/// </summary>
public static class Arg
{
    public static string CONFIG_PATH { get; private set; } = $"./data/{Const.CONFIG_FILE}";
    public static string CONFIG_BACKUP_PATH { get; private set; } = $"./data/{Const.CONFIG_FILE}.bak";
    public static bool DEBUG { get; private set; }

    // must be set before reading config.json
    private static void CommandLineArgs()
    {
        var root_command = new RootCommand("Command line keyword arguments.");

        var debug = new Option<bool>("--debug", "-d")
        {
            Description = "Debug mode used when developing",
            DefaultValueFactory = _ => false,
        };
        root_command.Add(debug);

        var config_path = new Option<string>("--config", "-c")
        {
            Description = "Custom config path",
            DefaultValueFactory = _ => $"./data/{Const.CONFIG_FILE}",
            Required = false
        };
        root_command.Add(config_path);

        root_command.SetAction(parse_result =>
        {
            var parsed_debug = parse_result.GetValue(debug);
            var parsed_config_path = parse_result.GetValue(config_path);

            if (parsed_debug)
                DEBUG = parsed_debug;

            if (!string.IsNullOrEmpty(parsed_config_path))
            {
                CONFIG_PATH = parsed_config_path;
            }
            else
            {
                throw new InvalidDataException("Invalid config path!");
            }
        });

        root_command.Parse(Environment.GetCommandLineArgs().Skip(1).ToArray()).Invoke();
    }

    /// <summary>
    /// Initializes cli args must be ran before accessing values.
    /// <list type="bullet">
    /// <item>debug - intended for testing bypasses & disables cli args</item>
    /// <item>config_path - intended for testing bypasses & disables cli args</item>
    /// </list>
    /// </summary>
    public static void Initialize(bool? debug = null, string? config_path = null)
    {
        if (debug == true) DEBUG = true;
        if (!String.IsNullOrEmpty(config_path)) CONFIG_PATH = config_path;

        if (debug == null && config_path == null) CommandLineArgs();
    }
}

/// <summary>
/// Uri type for JSON serialization converts Uri to string during serialization
/// and back to a Uri during deserialization.
/// </summary>
public class UriJsonConverter : JsonConverter<Uri>
{
    public override Uri? Read(ref Utf8JsonReader reader, Type convert_type, JsonSerializerOptions options)
    {
        var uri_string = reader.GetString();
        try
        {
            if (!string.IsNullOrEmpty(uri_string))
            {
                return new Uri(uri_string);
            }
        }
        catch { }
        throw new InvalidDataException($"Invalid url for value ({uri_string})");
    }

    public override void Write(Utf8JsonWriter writer, Uri value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

/// <summary>
/// Loads and validates config.json data.
/// <list type="bullet">
/// <item>Config.ARGS - config.json values</item>
/// <item>Config.LOCAL - network local ip or loopback</item>
/// <item>Config.Initialize() - set and validate config.json values</item>
/// </list>
/// </summary>
public static class Config
{
    public static string LOCAL_HOST { get; private set; } = Const.DEFAULT_ADDRESS;
    public static ConfigArgs ARGS { get; private set; } = null!;

    private static string LocalHost()
    {
        try
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            var address = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString();
            if (!string.IsNullOrEmpty(address))
            {
                if (!address.StartsWith("http://") && !address.StartsWith("https://"))
                {
                    return $"http://{address}";
                }
                return address;
            }
        }
        catch { }
        return Const.DEFAULT_ADDRESS;
    }

    private static Uri ParseUri(int port)
    {
        try
        {
            return new Uri($"{LOCAL_HOST}:{port}");

        }
        catch
        {
            throw new InvalidDataException("Parse adress failed invalid url provided!");
        }
    }

    private static ConfigArgs DefaultArgs()
    {
        return new ConfigArgs(
                ParseUri(Const.HOST_PORT),
                ParseUri(Const.SONARR_PORT),
                ParseUri(Const.RADARR_PORT),
                ParseUri(Const.JACKETT_PORT),
                ParseUri(Const.HYDRA_PORT),
                ParseUri(Const.RDTCLIENT_PORT),
                ParseUri(Const.SABNZBD_PORT),
                new string[0]
            );
    }

    private static void CheckArgs(ConfigArgs config)
    {
        var has_null = false;
        var properties = typeof(ConfigArgs).GetProperties();
        foreach (var arg in properties)
        {
            if (arg.GetValue(config) == null)
            {
                has_null = true;
                break;
            }
        }
        if (has_null)
        {
            var config_args = DefaultArgs();
            foreach (var arg in properties)
            {
                if (arg.GetValue(config) == null)
                {
                    arg.SetValue(config, arg.GetValue(config_args));
                }
            }
        }
    }

    private static void Validate()
    {
        var options = new JsonSerializerOptions()
        {
            WriteIndented = true,
            Converters = { new UriJsonConverter() }
        };

        string? json;
        try
        {
            json = File.ReadAllText(Arg.CONFIG_PATH);
        }
        catch
        {
            var config_path_directory = Path.GetDirectoryName(Arg.CONFIG_PATH);
            if (!string.IsNullOrEmpty(config_path_directory))
            {
                Directory.CreateDirectory(config_path_directory);
                ARGS = DefaultArgs();
                File.WriteAllText(Arg.CONFIG_PATH, JsonSerializer.Serialize(ARGS, options));
                return;
            }
            else
            {
                throw new InvalidDataException("Config path is invalid or does not exist!");
            }
        }

        if (!string.IsNullOrEmpty(json))
        {

            var config_args = JsonSerializer.Deserialize<ConfigArgs>(json, options);
            if (config_args != null)
            {
                // populate any missing args / nul
                var has_null = false;
                var properties = typeof(ConfigArgs).GetProperties();
                foreach (var arg in properties)
                {
                    if (arg.GetValue(config_args) == null)
                    {
                        has_null = true;
                        break;
                    }
                }
                if (has_null)
                {
                    var default_args = DefaultArgs();
                    foreach (var arg in properties)
                    {
                        if (arg.GetValue(config_args) == null)
                        {
                            arg.SetValue(config_args, arg.GetValue(default_args));
                        }
                    }
                    File.WriteAllText(Arg.CONFIG_BACKUP_PATH, json);
                    File.WriteAllText(Arg.CONFIG_PATH, JsonSerializer.Serialize(config_args, options));
                }
                ARGS = config_args;
            }
            else
            {
                throw new InvalidOperationException("Deserializing config.json failed!");
            }
        }
        else
        {
            throw new InvalidDataException("config.json is invalid or does not exist!");
        }
    }

    /// <summary>
    /// Initializes Config.ARGS must be ran before accessing values.
    /// </summary>
    public static void Initialize()
    {
        LOCAL_HOST = LocalHost();
        Validate();
    }
}
