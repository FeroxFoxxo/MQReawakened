using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Configs;
using Server.Base.Core.Events;
using Server.Base.Core.Services;
using Server.Base.Timers.Enums;
using Server.Base.Worlds.EventArguments;
using System.Collections;
using System.Diagnostics;
using System.IO.Compression;

namespace Server.Base.Worlds.Services;

public class ArchivedSaves : IService
{
    private readonly EventSink _eventSink;
    private readonly ILogger<ArchivedSaves> _logger;

    private readonly ServerHandler _serverHandler;
    private readonly InternalRConfig _config;

    private readonly AutoResetEvent _sync;
    private readonly object _taskRoot;
    private readonly List<IAsyncResult> _tasks;

    public ArchivedSaves(ILogger<ArchivedSaves> logger, EventSink eventSink, ServerHandler serverHandler,
        InternalRConfig config)
    {
        _logger = logger;
        _eventSink = eventSink;
        _serverHandler = serverHandler;
        _config = config;

        _sync = new AutoResetEvent(true);
        _tasks = new List<IAsyncResult>(config.BackupCapacity);
        _taskRoot = ((ICollection)_tasks).SyncRoot;
    }

    public void Initialize()
    {
        _eventSink.Shutdown += Wait;
        _eventSink.WorldSave += Wait;
    }

    public int GetPendingTasks()
    {
        lock (_taskRoot)
            return _tasks.Count - _tasks.RemoveAll(task => task.IsCompleted);
    }

    private void Wait(WorldSaveEventArgs worldSaveEventArgs) => WaitForTaskCompletion();

    private void Wait() => WaitForTaskCompletion();

    private void WaitForTaskCompletion()
    {
        if (!_serverHandler.HasCrashed && !_serverHandler.IsClosing)
            return;

        var pending = GetPendingTasks();

        if (pending <= 0)
            return;

        _logger.LogDebug("Waiting for {TaskCount} pending tasks...", pending);

        while (GetPendingTasks() > 0)
            _sync.WaitOne(10);

        _logger.LogDebug("All tasks completed.");
    }

    private void InternalPrune(DateTime threshold)
    {
        _logger.LogDebug("Pruning started...");

        var stopwatch = Stopwatch.StartNew();

        try
        {
            DirectoryInfo root = new(_config.ArchivedBackupDirectory);

            foreach (var archive in root.GetFiles("*.zip", SearchOption.AllDirectories))
            {
                try
                {
                    if (archive.LastWriteTimeUtc < threshold)
                        archive.Delete();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to delete archived saves");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to get files");
        }

        stopwatch.Stop();

        _logger.LogInformation("Pruning done in {Seconds} seconds.", stopwatch.Elapsed.TotalSeconds);
    }

    private void InternalPack(string source)
    {
        _logger.LogDebug("Packing started...");

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var now = DateTime.Now;

            var amOrPm = now.Hour < 12 ? "AM" : "PM";
            var twelveHours = now.Hour > 12 ? now.Hour - 12 : now.Hour <= 0 ? 12 : now.Hour;

            var date = _config.Merge switch
            {
                MergeType.Months => $"{now.Month}-{now.Year}",
                MergeType.Days => $"{now.Day}-{now.Month}-{now.Year}",
                MergeType.Hours => $"{now.Day}-{now.Month}-{now.Year} {twelveHours:D2} {amOrPm}",
                _ => $"{now.Day}-{now.Month}-{now.Year} {twelveHours:D2}-{now.Minute:D2} {amOrPm}"
            };

            var fileName = $"Saves ({date}).zip";
            var destinationName = Path.Combine(_config.ArchivedBackupDirectory, fileName);

            try
            {
                File.Delete(destinationName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get delete file {Name}", destinationName);
            }

            ZipFile.CreateFromDirectory(source, destinationName, CompressionLevel.Optimal, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to create zip file");
        }

        try
        {
            Directory.Delete(source, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to delete directory {Name}", source);
        }

        stopwatch.Stop();

        _logger.LogInformation("Packing done in {Seconds} seconds.", stopwatch.Elapsed.TotalSeconds);
    }

    public bool Process(string source)
    {
        if (_config.ExpireAge > TimeSpan.Zero)
            InternalPrune(DateTime.UtcNow - _config.ExpireAge);

        if (!string.IsNullOrWhiteSpace(source))
            InternalPack(source);

        return true;
    }
}
