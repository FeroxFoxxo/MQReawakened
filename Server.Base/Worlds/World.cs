using Microsoft.Extensions.Logging;
using Server.Base.Core.Events;
using Server.Base.Network.Services;
using Server.Base.Worlds.EventArguments;
using System.Diagnostics;

namespace Server.Base.Worlds;

public class World
{
    private readonly ManualResetEvent _diskWriteHandle;

    private readonly NetStateHandler _handler;
    private readonly ILogger<World> _logger;
    private readonly EventSink _sink;

    public bool Saving { get; private set; }
    public bool Loaded { get; private set; }
    public bool Loading { get; private set; }
    public bool Crashed { get; private set; }

    public World(ILogger<World> logger, EventSink sink, NetStateHandler handler)
    {
        _logger = logger;
        _sink = sink;
        _handler = handler;

        Saving = false;
        Loaded = false;
        Loading = false;
        Crashed = false;

        _diskWriteHandle = new ManualResetEvent(true);
    }

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

    public void WaitForWriteCompletion() => _diskWriteHandle.WaitOne();

    public void NotifyDiskWriteComplete()
    {
        if (_diskWriteHandle.Set())
            _logger.LogDebug("Closing save files.");
    }

    public void Save(bool message, bool permitBackgroundWrite)
    {
        if (Saving)
            return;

        _handler.Pause();

        WaitForWriteCompletion();

        Saving = true;

        _diskWriteHandle.Reset();

        _logger.LogInformation("Saving...");

        var stopWatch = Stopwatch.StartNew();

        try
        {
            _sink.InvokeWorldSave(new WorldSaveEventArgs(message));
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "FATAL: Exception in world save");
        }

        stopWatch.Stop();

        Saving = false;

        if (!permitBackgroundWrite)
            NotifyDiskWriteComplete();

        _logger.LogInformation("Save finished in {SECONDS} seconds.", stopWatch.Elapsed.TotalSeconds);

        _handler.Resume();
    }

    public void Broadcast(string message)
    {
        _sink.InvokeWorldBroadcast(new WorldBroadcastEventArgs(message));
        _logger.LogInformation("{MESSAGE}", message);
    }
}
