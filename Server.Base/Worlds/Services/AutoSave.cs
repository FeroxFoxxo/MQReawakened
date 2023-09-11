using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Configs;
using Server.Base.Core.Events;
using Server.Base.Core.Services;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;

namespace Server.Base.Worlds.Services;

public class AutoSave : IService
{
    private readonly TimeSpan _delayAutoSave;
    private readonly TimeSpan _delayWarning;

    private readonly ServerHandler _handler;
    private readonly EventSink _sink;
    private readonly TimerThread _timerThread;
    private readonly World _world;
    private readonly ILogger<AutoSave> _logger;
    private readonly InternalRConfig _config;
    private readonly ArchivedSaves _saves;

    public AutoSave(InternalRConfig config, ServerHandler handler, World world,
        EventSink sink, TimerThread timerThread, ILogger<AutoSave> logger, ArchivedSaves saves)
    {
        _handler = handler;
        _world = world;
        _sink = sink;
        _timerThread = timerThread;
        _logger = logger;
        _saves = saves;
        _config = config;

        _delayAutoSave = TimeSpan.FromMinutes(config.SaveAutomaticallyMinutes);
        _delayWarning = TimeSpan.FromMinutes(config.SaveWarningMinutes);
    }

    public void Initialize() => _sink.ServerStarted += _ => RunAutoSaveTimer();

    private void RunAutoSaveTimer()
    {
        var timer = _timerThread.DelayCall(Tick, _delayAutoSave - _delayWarning, _delayAutoSave, 0);
        timer.Stop();
    }

    public void Save()
    {
        _world.WaitForWriteCompletion();

        try
        {
            if (!Backup())
                _logger.LogError("Automatic backup failed");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Automatic backup failed");
        }

        _world.Save(true);
    }

    private void Tick()
    {
        if (_handler.Restarting)
            return;

        if (_delayWarning == TimeSpan.Zero)
        {
            Save();
        }
        else
        {
            var seconds = (int)_delayWarning.TotalSeconds;
            var minutes = seconds / 60;
            seconds %= 60;

            switch (minutes)
            {
                case > 0 when seconds > 0:
                    _world.Broadcast(
                        $"The world will save in {minutes} minute{(minutes != 1 ? "s" : string.Empty)} and {seconds} second{(seconds != 1 ? "s" : string.Empty)}.");
                    break;
                case > 0:
                    _world.Broadcast($"The world will save in {minutes} minute{(minutes != 1 ? "s" : string.Empty)}.");
                    break;
                default:
                    _world.Broadcast($"The world will save in {seconds} second{(seconds != 1 ? "s" : string.Empty)}.");
                    break;
            }

            _timerThread.DelayCall(Save, _delayWarning, TimeSpan.Zero, 1);
        }
    }

    private bool Backup()
    {
        if (_config.Backups.Length == 0)
            return false;

        var existing = Directory.GetDirectories(_config.AutomaticBackupDirectory);

        var anySuccess = existing.Length == 0;

        for (var i = 0; i < _config.Backups.Length; ++i)
        {
            var dir = Match(existing, _config.Backups[i]);

            if (dir == null)
                continue;

            if (i > 0)
            {
                try
                {
                    dir.MoveTo(Path.Combine(_config.AutomaticBackupDirectory, _config.Backups[i - 1]));

                    anySuccess = true;
                }
                catch (Exception e) { _logger.LogError(e, "Error backing up {BackupId} backup", i); }
            }
            else
            {
                var delete = true;

                try
                {
                    if (Directory.Exists(_config.TempBackupDirectory))
                        Directory.Delete(_config.TempBackupDirectory);

                    dir.MoveTo(_config.TempBackupDirectory);

                    delete = !_saves.Process(_config.TempBackupDirectory);
                }
                catch (Exception e) { _logger.LogError(e, "Error backing up {BackupId} backup", i); }

                if (!delete)
                    continue;

                try { dir.Delete(true); }
                catch (Exception e) { _logger.LogError(e, "Error backing up {BackupId} backup", i); }
            }
        }

        if (Directory.Exists(_config.SaveDirectory))
            Directory.Move(_config.SaveDirectory, Path.Combine(_config.AutomaticBackupDirectory, _config.Backups[^1]));

        return anySuccess;
    }

    private static DirectoryInfo Match(IEnumerable<string> paths, string match) =>
        paths.Select(t => new DirectoryInfo(t)).FirstOrDefault(info => info.Name.StartsWith(match));
}
