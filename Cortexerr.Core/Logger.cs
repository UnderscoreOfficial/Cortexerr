using Cortexerr.Core.Configuration;
using Serilog;
using SerilogLog = Serilog.Log;

namespace Cortexerr.Core.Logging;

public static class Logger
{
    public static ILogger Log => SerilogLog.Logger;

    public static void Initialize()
    {
        var config = new LoggerConfiguration();
        if (Arg.DEBUG)
        {
            config.MinimumLevel.Debug();
        }
        else
        {
            config.MinimumLevel.Information();
        }
        config
            .WriteTo.Console()
            .WriteTo.File("logs/app-.log",
                          rollingInterval: RollingInterval.Day,
                          retainedFileCountLimit: 30);
        SerilogLog.Logger = config.CreateLogger();
    }
}
