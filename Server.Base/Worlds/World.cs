using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Base.Network.Services;
using Server.Base.Worlds.EventArguments;
using System.Diagnostics;

namespace Server.Base.Worlds;

public class World(ILogger<World> logger, IServiceProvider services, ServerHandler serverHandler,
    EventSink sink, NetStateHandler netStateHandler) : IService
{
    public bool Saving { get; private set; } = false;
    public bool Loaded { get; private set; } = false;
    public bool Loading { get; private set; } = false;
    public bool Crashed { get; private set; } = false;

    public void Initialize() { }

    public void Load()
    {
        if (Loaded)
            return;

        Loaded = true;

        logger.LogInformation("Loading world...");

        var stopWatch = Stopwatch.StartNew();

        Loading = true;

        try
        {
            sink.InvokeWorldLoad();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "FATAL: Exception in world load");
            Crashed = true;
        }

        Loading = false;

        stopWatch.Stop();

        logger.LogInformation("Finished loading in {SECONDS} seconds.", stopWatch.Elapsed.TotalSeconds);
    }

    public void Save(bool message)
    {
        if (Saving)
        {
            Broadcast("The world is already saving, please wait.");
            return;
        }

        if (message)
            Broadcast("The world is saving, please wait.");

        netStateHandler.Pause();

        Saving = true;

        var watch = Stopwatch.StartNew();

        try
        {
            services.SaveConfigs(serverHandler.Modules, logger);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "FATAL: Exception in world save configs");
        }

        try
        {
            sink.InvokeWorldSave(new WorldSaveEventArgs(message));
        }
        catch (Exception e)
        {
            throw new Exception("FATAL: Exception in world save event", e);
        }

        watch.Stop();

        Saving = false;

        netStateHandler.Resume();

        logger.LogInformation("Save finished in {Time:F2} seconds.", watch.Elapsed.TotalSeconds);

        if (message)
            Broadcast($"World save done in {watch.Elapsed.TotalSeconds:F2} seconds.");
    }

    public void Broadcast(string message)
    {
        try
        {
            sink.InvokeWorldBroadcast(new WorldBroadcastEventArgs(message));
            logger.LogInformation("{MESSAGE}", message);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "FATAL: Exception in world broadcast");
        }
    }
}
