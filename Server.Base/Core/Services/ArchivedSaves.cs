using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Configs;
using Server.Base.Core.Events;
using Server.Base.Timers.Enums;
using Server.Base.Worlds.EventArguments;
using System.Collections;
using System.Diagnostics;
using System.IO.Compression;

namespace Server.Base.Core.Services;

public class ArchivedSaves(EventSink sink, InternalRConfig rConfig, InternalRwConfig rwConfig,
    ServerHandler handler, ILogger<ArchivedSaves> logger) : IService
{
    private readonly List<Task> _tasks = new(0x40);
    private object _taskRoot;

    private readonly AutoResetEvent _sync = new(true);

    private Action<string> _pack;
    private Action<DateTime> _prune;

    public int PendingTasks
    {
        get
        {
            lock (_taskRoot)
                return _tasks.Count - _tasks.RemoveAll(t => t.IsCompleted);
        }
    }

    public void Initialize()
    {
        sink.Shutdown += Wait;
        sink.WorldSave += Wait;

        _pack = InternalPack;
        _prune = InternalPrune;
        _taskRoot = ((ICollection)_tasks).SyncRoot;
    }

    public void Process(string source)
    {
        if (!Directory.Exists(rConfig.ArchivedBackupDirectory))
            Directory.CreateDirectory(rConfig.ArchivedBackupDirectory);

        if (rConfig.ExpireAge > TimeSpan.Zero)
            BeginPrune(DateTime.UtcNow - rConfig.ExpireAge);

        if (!string.IsNullOrWhiteSpace(source))
            BeginPack(source);
    }

    private void Wait(WorldSaveEventArgs e) => WaitForTaskCompletion();

    private void Wait() => WaitForTaskCompletion();

    private void WaitForTaskCompletion()
    {
        if (!handler.HasCrashed && !handler.IsClosing)
            return;

        var pending = PendingTasks;

        if (pending <= 0)
            return;

        logger.LogInformation("Archives: Waiting for {Seconds:#,0} pending tasks...", pending);

        while (pending > 0)
        {
            _sync.WaitOne(10);

            pending = PendingTasks;
        }

        logger.LogInformation("Archives: All tasks completed.");
    }

    private void InternalPack(string source)
    {
        logger.LogInformation("Archives: Packing started...");

        var sw = Stopwatch.StartNew();

        try
        {
            var now = DateTime.Now;

            var ampm = now.Hour < 12 ? "AM" : "PM";
            var hour12 = now.Hour > 12 ? now.Hour - 12 : now.Hour <= 0 ? 12 : now.Hour;

            var date = rConfig.Merge switch
            {
                MergeType.Months => string.Format("{0}-{1}", now.Month, now.Year),
                MergeType.Days => string.Format("{0}-{1}-{2}", now.Day, now.Month, now.Year),
                MergeType.Hours => string.Format("{0}-{1}-{2} {3:D2} {4}", now.Day, now.Month, now.Year, hour12, ampm),
                _ => string.Format("{0}-{1}-{2} {3:D2}-{4:D2} {5}", now.Day, now.Month, now.Year, hour12, now.Minute, ampm),
            };

            var file = string.Format("{0} Saves ({1}).zip", rwConfig.ServerName, date);
            var dest = Path.Combine(rConfig.ArchivedBackupDirectory, file);

            try { File.Delete(dest); }
            catch { }

            ZipFile.CreateFromDirectory(source, dest, CompressionLevel.Optimal, false);
        }
        catch { }

        try { Directory.Delete(source, true); }
        catch { }

        sw.Stop();

        logger.LogDebug("Packing done in {Seconds:F1} seconds.", sw.Elapsed.TotalSeconds);
    }

    private void BeginPack(string source)
    {
        if (handler.HasCrashed || handler.IsClosing)
        {
            _pack.Invoke(source);
            return;
        }

        _sync.Reset();

        var t = Task.Run(() => _pack(source));
        t.ContinueWith(EndPack);

        lock (_taskRoot)
            _tasks.Add(t);

        t.Wait();
    }

    private void EndPack(Task t)
    {
        if (t.IsFaulted)
            logger.LogError("Error while packing: {Message}", t.Exception.Message);

        lock (_taskRoot)
            _tasks.Remove(t);

        _sync.Set();
    }

    private void InternalPrune(DateTime threshold)
    {
        if (!Directory.Exists(rConfig.ArchivedBackupDirectory))
            return;

        logger.LogInformation("Archives: Pruning started...");

        var sw = Stopwatch.StartNew();

        try
        {
            var root = new DirectoryInfo(rConfig.ArchivedBackupDirectory);

            foreach (var archive in root.GetFiles("*.zip", SearchOption.AllDirectories))
            {
                try
                {
                    if (archive.LastWriteTimeUtc < threshold)
                        archive.Delete();
                }
                catch (Exception e) {
                    logger.LogError("Error while deleting archive: {Message}", e.Message);
                }
            }
        }
        catch { }

        sw.Stop();

        logger.LogDebug("Pruning done in {seconds:F1} seconds.", sw.Elapsed.TotalSeconds);
    }

    private void BeginPrune(DateTime threshold)
    {
        if (handler.HasCrashed || handler.IsClosing)
        {
            _prune.Invoke(threshold);
            return;
        }

        _sync.Reset();

        var t = Task.Run(() => _prune(threshold));
        t.ContinueWith(EndPrune);

        lock (_taskRoot)
            _tasks.Add(t);

        t.Wait();
    }

    private void EndPrune(Task t)
    {
        if (t.IsFaulted)
            logger.LogError("Error while pruning: {Message}", t.Exception.Message);

        lock (_taskRoot)
            _tasks.Remove(t);

        _sync.Set();
    }
}
