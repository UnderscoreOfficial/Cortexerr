using Cortexerr.Core.Configuration;
using Cortexerr.Core.Logging;
using System.Runtime.CompilerServices;

namespace Cortexerr.Tests.Unit;

public static class Initialize
{
    [ModuleInitializer]
    public static void Load()
    {
        Arg.Initialize(true);
        Logger.Initialize();
        Env.Initialize();
        Config.Initialize();
        Console.WriteLine(Arg.DEBUG);
    }
}
