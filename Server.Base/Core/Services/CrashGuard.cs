using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Models;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Events.Arguments;
using Server.Base.Core.Extensions;
using Server.Base.Core.Models;
using Server.Base.Network.Services;
using Server.Base.Worlds;
using System.Diagnostics;

namespace Server.Base.Core.Services;

public class CrashGuard : IService
{
    private readonly NetStateHandler _handler;
    private readonly ILogger<CrashGuard> _logger;
    private readonly Module[] _modules;
    private readonly EventSink _sink;
    private readonly World _world;
    private readonly InternalRConfig _config;

    public CrashGuard(NetStateHandler handler, ILogger<CrashGuard> logger, EventSink sink, World world,
        IServiceProvider services, InternalRConfig config)
    {
        _handler = handler;
        _logger = logger;
        _sink = sink;
        _world = world;
        _config = config;

        _modules = services.GetServices<Module>().ToArray();
    }

    public void Initialize() => _sink.Crashed += OnCrash;

    public void OnCrash(CrashedEventArgs e)
    {
        GenerateCrashReport(e);

        _world.WaitForWriteCompletion();

        Backup();

        Restart(e);
    }

    private void Restart(CrashedEventArgs e)
    {
        _logger.LogDebug("Restarting...");

        try
        {
            Process.Start(GetExePath.Path());
            _logger.LogInformation("Successfully restarted!");

            e.Close = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restart server");
        }
    }

    private void Backup()
    {
        _logger.LogDebug("Backing up...");

        try
        {
            var timeStamp = GetTime.GetTimeStamp();

            var backup = Path.Combine(_config.CrashBackupDirectory, timeStamp);

            InternalDirectory.CreateDirectory(backup);

            CopyFiles(_config.SaveDirectory, _config.CrashBackupDirectory);

            _logger.LogInformation("Backed up!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to back up server.");
        }
    }

    private void CopyFiles(string originPath, string backupPath)
    {
        try
        {
            foreach (var fileLink in Directory.GetFiles(originPath))
            {
                var file = Path.GetFileName(fileLink);
                var oldF = Path.Combine(originPath, file);
                var newF = Path.Combine(backupPath, file);
                File.Copy(oldF, newF, true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to copy file.");
        }
    }

    private void GenerateCrashReport(CrashedEventArgs crashedEventArgs)
    {
        _logger.LogDebug("Generating report...");

        try
        {
            var timeStamp = GetTime.GetTimeStamp();
            var fileName = $"Crash {timeStamp}.log";
            
            var filePath = Path.Combine(_config.CrashDirectory, fileName);

            using (var streamWriter = new StreamWriter(filePath))
            {
                streamWriter.WriteLine("Server Crash Report");
                streamWriter.WriteLine("===================");
                streamWriter.WriteLine();

                foreach (var module in _modules)
                    streamWriter.WriteLine(module.GetModuleInformation());
                streamWriter.WriteLine("Operating System: {0}", Environment.OSVersion);
                streamWriter.WriteLine(".NET Framework: {0}", Environment.Version);
                streamWriter.WriteLine("Time: {0}", DateTime.UtcNow);

                streamWriter.WriteLine();
                streamWriter.WriteLine("Exception:");
                streamWriter.WriteLine(crashedEventArgs.Exception);
                streamWriter.WriteLine();

                streamWriter.WriteLine("Clients:");

                try
                {
                    var netStates = _handler.Instances;

                    streamWriter.WriteLine("- Count: {0}", netStates.Count);

                    foreach (var netState in netStates)
                    {
                        streamWriter.Write("+ {0}:", netState);

                        var account = netState.Get<Account>();

                        if (account != null)
                            streamWriter.Write(" (Account = {0})", account.Username);

                        streamWriter.WriteLine();
                    }
                }
                catch
                {
                    streamWriter.WriteLine("- Failed");
                }
            }


            _logger.LogInformation("Logged error!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to log error.");
        }
    }
}
