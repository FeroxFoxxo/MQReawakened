using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Configs;
using Server.Base.Core.Events;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Base.Worlds;

namespace Server.Base.Core.Services;
public class AutoSave(TimerThread timerThread, InternalRConfig config, World world, EventSink sink, ArchivedSaves archivedSaves, ILogger<AutoSave> logger) : IService
{
    public void Initialize() =>
        sink.ServerStarted += (_) =>
            timerThread.RunIndefiniteDelayedInterval(Tick, null, config.SaveAutomatically, config.SaveAutomatically - config.SaveWarning);

    public void Save(object _)
    {
        if (!world.Loaded)
            return;

        logger.LogInformation("Running auto save...");

        try
        {
            if (!Backup())
                logger.LogError("WARNING: Automatic backup FAILED");
        }
        catch (Exception e)
        {
            logger.LogError("WARNING: Automatic backup FAILED:\n{Error}", e);
        }

        world.Save(true);

        logger.LogDebug("Completed auto save.");
    }

    public void Tick(object _)
    {
        if (!world.Loaded)
            return;

        if (config.SaveWarning == TimeSpan.Zero)
        {
            Save(null);
        }
        else
        {
            var s = (int)config.SaveWarning.TotalSeconds;
            var m = s / 60;
            s %= 60;

            if (m > 0 && s > 0)
                world.Broadcast(string.Format("The world will save in {0} minute{1} and {2} second{3}.", m, m != 1 ? "s" : "", s, s != 1 ? "s" : ""));
            else if (m > 0)
                world.Broadcast(string.Format("The world will save in {0} minute{1}.", m, m != 1 ? "s" : ""));
            else
                world.Broadcast(string.Format("The world will save in {0} second{1}.", s, s != 1 ? "s" : ""));

            timerThread.RunDelayed(Save, null, config.SaveWarning);
        }
    }

    private bool Backup()
    {
        if (config.Backups.Length == 0)
            return false;

        var root = config.AutomaticBackupDirectory;

        if (!Directory.Exists(root))
            Directory.CreateDirectory(root);

        var tempRoot = config.TempBackupDirectory;

        if (Directory.Exists(tempRoot))
            Directory.Delete(tempRoot, true);

        var existing = Directory.GetDirectories(root);

        var anySuccess = existing.Length == 0;

        logger.LogInformation("Backing up save files...");

        for (var i = 0; i < config.Backups.Length; ++i)
        {
            var dir = Match(existing, config.Backups[i]);

            if (dir == null)
                continue;

            if (i > 0)
            {
                try
                {
                    dir.MoveTo(Path.Combine(root, config.Backups[i - 1]));

                    anySuccess = true;
                }
                catch (Exception e)
                {
                    logger.LogError("Exception while moving backups: {Exception}", e.Message);
                }
            }
            else
            {
                var delete = true;

                try
                {
                    dir.MoveTo(tempRoot);

                    archivedSaves.Process(tempRoot);

                    delete = false;
                }
                catch (Exception e)
                {
                    logger.LogError("Exception while moving and processing backups: {Exception}", e.Message);
                }

                if (delete)
                {
                    try { dir.Delete(true); }
                    catch { }
                }
            }
        }

        CopyFiles(config.SaveDirectory, Path.Combine(root, config.Backups[^1]), logger);

        return anySuccess;
    }

    public static void CopyFiles(string sourceDir, string destDir, ILogger logger)
    {
        if (Directory.Exists(sourceDir))
        {
            Directory.CreateDirectory(destDir);

            var files = Directory.GetFiles(sourceDir);

            foreach (var file in files)
            {
                try
                {
                    var fileName = Path.GetFileName(file);
                    var destFile = Path.Combine(destDir, fileName);

                    File.Copy(file, destFile, true);
                }
                catch (IOException ex)
                {
                    logger.LogError("Error copying file {File}: {Message}", file, ex.Message);
                }
            }
        }
    }

    private static DirectoryInfo Match(string[] paths, string match)
    {
        for (var i = 0; i < paths.Length; ++i)
        {
            var info = new DirectoryInfo(paths[i]);

            if (info.Name.StartsWith(match))
                return info;
        }

        return null;
    }
}
