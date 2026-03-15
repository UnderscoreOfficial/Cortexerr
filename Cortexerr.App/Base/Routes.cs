using Cortexerr.Core.Configuration;

namespace Cortexerr.App.Base;

public static class Routes
{
    public static readonly WebApplicationBuilder builder = WebApplication.CreateBuilder();
    public static readonly WebApplication app = builder.Build();

    public static void Initialize()
    {
        app.MapTorznabRoutes();
        app.MapQbittorrentRoutes();
        app.Run(Config.ARGS.host.ToString());
    }
}
