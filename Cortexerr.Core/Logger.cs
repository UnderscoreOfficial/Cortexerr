using Cortexerr.Core.Configuration;
using Serilog.Templates;
using Serilog.Templates.Themes;
using Serilog;
using SerilogLog = Serilog.Log;
using Serilog.Sinks.SystemConsole.Themes;

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

        var theme = new TemplateTheme(new Dictionary<TemplateThemeStyle, string>
        {
            [TemplateThemeStyle.Text] = "\x1b[38;5;0253m",
            [TemplateThemeStyle.SecondaryText] = "\x1b[38;5;0238m",
            [TemplateThemeStyle.LevelDebug] = "\x1b[38;5;0039m\x1b[48;5;0234m",  // blue
            [TemplateThemeStyle.LevelInformation] = "\x1b[38;5;0034m\x1b[48;5;0234m",  // green
            [TemplateThemeStyle.LevelWarning] = "\x1b[38;5;0220m\x1b[48;5;0234m",  // yellow
            [TemplateThemeStyle.LevelError] = "\x1b[38;5;0124m\x1b[48;5;0234m",  // red
            [TemplateThemeStyle.LevelFatal] = "\x1b[38;5;0093m\x1b[48;5;0234m", // purple
        });

        var console_formatter = new ExpressionTemplate("{@l:w4}" + ": {@m}\n{@x}", theme: theme);
        var file_formatter = new ExpressionTemplate("{@t:HH:mm:ss} " + "{@l:w4}" + ": {@m}\n{@x}");

        config
                .WriteTo.Console(console_formatter)
                .WriteTo.File(file_formatter, "logs/cortexerr-.log",
                              rollingInterval: RollingInterval.Day,
                              retainedFileCountLimit: 30);

        SerilogLog.Logger = config.CreateLogger();
    }
}
