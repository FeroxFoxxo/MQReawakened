using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Configs;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Base.Network.Events;
using Server.Base.Network.Services;
using Server.Base.Worlds.EventArguments;
using System.Diagnostics;

namespace Server.Base.Worlds;

public class World(ILogger<World> logger, IServiceProvider services, ServerHandler serverHandler,
    EventSink sink, InternalRConfig config, NetStateHandler netStateHandler) : IService
{
    private readonly ManualResetEvent _diskWriteHandle = new(true);

    public bool Saving { get; private set; } = false;
    public bool Loaded { get; private set; } = false;
    public bool Loading { get; private set; } = false;
    public bool Crashed { get; private set; } = false;

    private DateTime _lastSave = DateTime.Now;

    public void Initialize() => sink.NetStateRemoved += TrySaveWorld;

    private void TrySaveWorld(NetStateRemovedEventArgs @event)
    {
        if (_lastSave + config.SaveRateLimit > DateTime.Now)
            return;

        Save(true);
        _lastSave = DateTime.Now;
    }

    public void NotifyDiskWriteComplete()
    {
        if (_diskWriteHandle.Set())
            logger.LogInformation("Closing Save Files. ");
    }

    public void WaitForWriteCompletion() => _diskWriteHandle.WaitOne();

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

        netStateHandler.Pause();

        WaitForWriteCompletion();

        Saving = true;

        _diskWriteHandle.Reset();

        if (message)
            Broadcast("The world is saving, please wait.");

        var watch = Stopwatch.StartNew();

        InternalDirectory.CreateDirectory(config.SaveDirectory);

        try
        {
            sink.InvokeWorldSave(new WorldSaveEventArgs(message));

            services.SaveConfigs(serverHandler.Modules, logger);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "FATAL: Exception in world save");
        }

        watch.Stop();

        Saving = false;

        NotifyDiskWriteComplete();

        logger.LogInformation("Save finished in {Time:F2} seconds.", watch.Elapsed.TotalSeconds);

        try
        {
            if (message)
                Broadcast($"World save done in {watch.Elapsed.TotalSeconds} seconds.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "FATAL: Exception in world broadcast");
        }

        netStateHandler.Resume();
    }

    public void Broadcast(string message)
    {
        sink.InvokeWorldBroadcast(new WorldBroadcastEventArgs(message));
        logger.LogInformation("{MESSAGE}", message);
    }
}
