using System.Reflection;
using Cortexerr.Core.Configuration;
using Cortexerr.Core.Errors;
using Cortexerr.Core.Ingest;
using Cortexerr.Core.Logging;
using Cortexerr.Decisions.Consumer;
using Cortexerr.Decisions.Logic;

namespace Cortexerr.App.Base;

public static class State
{
    public static IIngestConsumer consumer { get; private set; } = null!;
    public static DecisionLogic logic { get; private set; } = null!;

    private static HandleResponse<List<Assembly>> LoadDllAssemblies()
    {
        if (!Directory.Exists(Config.ARGS.custom_dll_path))
        {
            return
                Response.Error<List<Assembly>>(ErrorCode.INVALID_INPUT, "(State|LoadDllAssemblies) Invalid custom dll path");
        }

        var dlls = Directory.EnumerateFiles(Config.ARGS.custom_dll_path, "*.dll");
        var assemblies = new List<Assembly>();

        foreach (var dll in dlls)
        {
            var asm = Error.Handle(() => Assembly.LoadFrom(dll));
            if (asm.data != null)
            {
                assemblies.Add(asm.data);
            }
            else
            {
                Logger.Log.Error($"(State|LoadDllAssemblies) Failed to load ({dll})");
            }
        }
        if (assemblies.Count > 0)
        {
            return Response.Success(assemblies);
        }
        return
            Response.Error<List<Assembly>>(ErrorCode.INVALID_INPUT, "(State|LoadDllAssemblies) No assemblies provided");
    }

    private static DecisionLogic DecisionLogicBuilder(HandleResponse<List<Assembly>> assemblies)
    {
        if (Config.ARGS.custom_dll_api_level == DllApiLevel.OVERRIDE)
        {
            var base_type = typeof(DecisionLogic);
            if (assemblies.data != null)
            {
                var canidates = assemblies.data
                    .SelectMany(asm => asm.GetTypes())
                    .Where(type =>
                            !type.IsAbstract &&
                            base_type.IsAssignableFrom(type) &&
                            type != base_type)
                    .ToArray();
                if (canidates.Length == 0)
                    return new DecisionLogic();
                if (canidates.Length > 1)
                {
                    Logger.Log.Warning("(State|DecisionLogicBuilder) Multiple DecisionLogic canidates, DecisionLogic may be wrong");
                }

                var instance = Activator.CreateInstance(canidates[0]);
                if (instance is DecisionLogic logic)
                    return logic;

                Logger.Log.Error("(State|DecisionLogicBuilder) Instance type of DecisionLogic");
            }
        }
        return new DecisionLogic();
    }

    private static IIngestConsumer IngestConsumerBuilder(HandleResponse<List<Assembly>> assemblies)
    {
        if (Config.ARGS.custom_dll_api_level == DllApiLevel.FULL)
        {
            var base_type = typeof(IIngestConsumer);
            if (assemblies.data != null)
            {
                var canidates = assemblies.data
                    .SelectMany(asm => asm.GetTypes())
                    .Where(type =>
                            !type.IsAbstract &&
                            base_type.IsAssignableFrom(type))
                    .ToArray();
                if (canidates.Length == 0)
                    return new IngestConsumer();
                if (canidates.Length > 1)
                {
                    Logger.Log.Warning("(State|IngestConsumerBuilder) Multiple IIngestConsumer canidates, consumer may be wrong");
                }

                var instance = Activator.CreateInstance(canidates[0]);
                if (instance is IIngestConsumer consumer)
                    return consumer;

                Logger.Log.Error("(State|IngestConsumerBuilder) Instance type of IIngestConsumer");
            }
        }
        return new IngestConsumer();
    }

    public static void Initialize()
    {
        var assemblies = LoadDllAssemblies();
        consumer = IngestConsumerBuilder(assemblies);
        logic = DecisionLogicBuilder(assemblies);
    }
}
