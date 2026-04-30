using System.Text.Json;
using Cortexerr.Core.Configuration;

namespace Cortexerr.App.Base;

public static class SabnzbdRoutes
{
    public static WebApplication MapSabnzbdRoutes(this WebApplication app)
    {
        app.MapGet("/api", async (string? mode, string? apikey, string? output) =>
        {
            Console.WriteLine("mode: " + mode);
            Console.WriteLine("apikey: " + apikey);
            Console.WriteLine("output: " + output);
            if (apikey != Config.ARGS.host_api_key)
                return Results.StatusCode(StatusCodes.Status401Unauthorized);
            if (mode == "version")
                return Results.Json(new { version = "4.3.2" });
            if (mode == "get_config")
            {
                var config = new
                {
                    config = new
                    {
                        misc = new
                        {
                            history_retention = "",
                            history_retention_option = "all",
                            history_retention_number = 1,
                            complete_dir = "/downloads",
                            enable_tv_sorting = 0,
                            tv_categories = new[] { "tv" },
                            enable_movie_sorting = 0,
                            movie_categories = new[] { "movies" },
                            enable_date_sorting = 0,
                            date_categories = new[] { "date" },
                            pre_check = 0,
                        },
                        categories = new[]
                    {
                        new { Name = "*",      Priority = 0,    PP = "3", Script = "None",    Dir = "" },
                        new { Name = "tv",     Priority = -100, PP = "",  Script = "Default", Dir = "" },
                        new { Name = "movies", Priority = -100, PP = "",  Script = "Default", Dir = "" },
                    },
                        sorters = new object[] { }
                    }
                };
                Console.WriteLine(JsonSerializer.Serialize(config));
                return Results.Json(config);
            }
            return Results.StatusCode(StatusCodes.Status404NotFound);
        });

        return app;
    }
}

