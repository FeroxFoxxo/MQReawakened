using Microsoft.Extensions.Logging;
using Server.Base.Core.Events;
using Server.Base.Worlds.EventArguments;
using System.Diagnostics;

namespace Server.Base.Worlds;

public class World
{
    private readonly ILogger<World> _logger;
    private readonly EventSink _sink;

    public bool Saving { get; private set; }
    public bool Loaded { get; private set; }
    public bool Loading { get; private set; }
    public bool Crashed { get; private set; }

    public World(ILogger<World> logger, EventSink sink)
    {
        _logger = logger;
        _sink = sink;

        Saving = false;
        Loaded = false;
        Loading = false;
        Crashed = false;
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

    public void Save(bool message, bool _)
    {
        try
        {
            _sink.InvokeWorldSave(new WorldSaveEventArgs(message));
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "FATAL: Exception in world save");
        }
    }

    public void Broadcast(string message)
    {
        _sink.InvokeWorldBroadcast(new WorldBroadcastEventArgs(message));
        _logger.LogInformation("{MESSAGE}", message);
    }
}
