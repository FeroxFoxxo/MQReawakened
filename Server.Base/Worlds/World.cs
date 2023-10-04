using Microsoft.Extensions.Logging;
using Server.Base.Core.Configs;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Network.Services;
using Server.Base.Worlds.EventArguments;
using System.Diagnostics;

namespace Server.Base.Worlds;

public class World
{
    private readonly ILogger<World> _logger;
    private readonly EventSink _sink;
    private readonly InternalRConfig _config;
    private readonly NetStateHandler _netStateHandler;

    private readonly ManualResetEvent _diskWriteHandle;

    public bool Saving { get; private set; }
    public bool Loaded { get; private set; }
    public bool Loading { get; private set; }
    public bool Crashed { get; private set; }

    public World(ILogger<World> logger, EventSink sink, InternalRConfig config, NetStateHandler netStateHandler)
    {
        _logger = logger;
        _sink = sink;
        _config = config;
        _netStateHandler = netStateHandler;

        _diskWriteHandle = new ManualResetEvent(true);

        Saving = false;
        Loaded = false;
        Loading = false;
        Crashed = false;
    }

    public void NotifyDiskWriteComplete()
    {
        if (_diskWriteHandle.Set())
            _logger.LogInformation("Closing Save Files. ");
    }

    public void WaitForWriteCompletion() => _diskWriteHandle.WaitOne();

    public void Load()
    {
        if (Loaded)
            return;

        Loaded = true;

        _logger.LogInformation("Loading world...");

        var stopWatch = Stopwatch.StartNew();

        Loading = true;

        try
        {
            _sink.InvokeWorldLoad();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "FATAL: Exception in world load");
            Crashed = true;
        }

        Loading = false;

        stopWatch.Stop();

        _logger.LogInformation("Finished loading in {SECONDS} seconds.", stopWatch.Elapsed.TotalSeconds);
    }

    public void Save(bool message)
    {
        if (Saving)
            return;

        _netStateHandler.Pause();

        WaitForWriteCompletion();

        Saving = true;

        _diskWriteHandle.Reset();

        if (message)
            Broadcast("The world is saving, please wait.");

        var watch = Stopwatch.StartNew();

        InternalDirectory.CreateDirectory(_config.SaveDirectory);

        try
        {
            _sink.InvokeWorldSave(new WorldSaveEventArgs(message));
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "FATAL: Exception in world save");
        }

        watch.Stop();

        Saving = false;

        NotifyDiskWriteComplete();

        _logger.LogInformation("Save finished in {Time:F2} seconds.", watch.Elapsed.TotalSeconds);

        if (message)
        {
            Broadcast($"World save done in {watch.Elapsed.TotalSeconds} seconds.");
        }

        _netStateHandler.Resume();
    }

    public void Broadcast(string message)
    {
        _sink.InvokeWorldBroadcast(new WorldBroadcastEventArgs(message));
        _logger.LogInformation("{MESSAGE}", message);
    }
}
