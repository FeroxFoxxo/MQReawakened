using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Configs;
using Server.Base.Core.Events;
using Server.Base.Core.Services;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;

namespace Server.Base.Worlds.Services;

public class AutoSave(InternalRConfig config, ServerHandler handler, World world,
    EventSink sink, TimerThread timerThread, ILogger<AutoSave> logger, ArchivedSaves saves) : IService
{
    private readonly TimeSpan _delayAutoSave = TimeSpan.FromMinutes(config.SaveAutomaticallyMinutes);
    private readonly TimeSpan _delayWarning = TimeSpan.FromMinutes(config.SaveWarningMinutes);

    public void Initialize() => sink.ServerStarted += _ => RunAutoSaveTimer();

    private void RunAutoSaveTimer()
    {
        var timer = timerThread.DelayCall(Tick, null, _delayAutoSave - _delayWarning, _delayAutoSave, 0);
        timer.Stop();
    }

    public void Save()
    {
        world.WaitForWriteCompletion();

        try
        {
            if (!Backup())
                logger.LogError("Automatic backup failed");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Automatic backup failed");
        }

        world.Save(true);
    }

    private void Tick(object _)
    {
        if (handler.Restarting)
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
                    world.Broadcast(
                        $"The world will save in {minutes} minute{(minutes != 1 ? "s" : string.Empty)} and {seconds} second{(seconds != 1 ? "s" : string.Empty)}.");
                    break;
                case > 0:
                    world.Broadcast($"The world will save in {minutes} minute{(minutes != 1 ? "s" : string.Empty)}.");
                    break;
                default:
                    world.Broadcast($"The world will save in {seconds} second{(seconds != 1 ? "s" : string.Empty)}.");
                    break;
            }

            timerThread.DelayCall((object _) => Save(), null, _delayWarning, TimeSpan.Zero, 1);
        }
    }

    private bool Backup()
    {
        if (config.Backups.Length == 0)
            return false;

        var existing = Directory.GetDirectories(config.AutomaticBackupDirectory);

        var anySuccess = existing.Length == 0;

        for (var i = 0; i < config.Backups.Length; ++i)
        {
            var dir = Match(existing, config.Backups[i]);

            if (dir == null)
                continue;

            if (i > 0)
            {
                try
                {
                    dir.MoveTo(Path.Combine(config.AutomaticBackupDirectory, config.Backups[i - 1]));

                    anySuccess = true;
                }
                catch (Exception e) { logger.LogError(e, "Error backing up {BackupId} backup", i); }
            }
            else
            {
                var delete = true;

                try
                {
                    if (Directory.Exists(config.TempBackupDirectory))
                        Directory.Delete(config.TempBackupDirectory);

                    dir.MoveTo(config.TempBackupDirectory);

                    delete = !saves.Process(config.TempBackupDirectory);
                }
                catch (Exception e) { logger.LogError(e, "Error backing up {BackupId} backup", i); }

                if (!delete)
                    continue;

                try { dir.Delete(true); }
                catch (Exception e) { logger.LogError(e, "Error backing up {BackupId} backup", i); }
            }
        }

        if (Directory.Exists(config.SaveDirectory))
            Directory.Move(config.SaveDirectory, Path.Combine(config.AutomaticBackupDirectory, config.Backups[^1]));

        return anySuccess;
    }

    private static DirectoryInfo Match(IEnumerable<string> paths, string match) =>
        paths.Select(t => new DirectoryInfo(t)).FirstOrDefault(info => info.Name.StartsWith(match));
}
