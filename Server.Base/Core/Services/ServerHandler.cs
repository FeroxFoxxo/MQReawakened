using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Events.Arguments;
using Server.Base.Core.Extensions;
using System.Diagnostics;

namespace Server.Base.Core.Services;

public class ServerHandler(EventSink sink, ILogger<ServerHandler> logger, IHostApplicationLifetime appLifetime) : IService
{
    private readonly AutoResetEvent _signal = new(true);

    public bool HasCrashed = false;
    public bool IsClosing = false;
    public IEnumerable<Module> Modules;
    public bool Restarting = false;
    public bool Saving = false;

    public void Initialize()
    {
        AppDomain.CurrentDomain.UnhandledException += UnhandledException;
        appLifetime.ApplicationStopped.Register(HandleClosed);
    }

    public void SetModules(IEnumerable<Module> modules) => Modules = modules;

    public void UnhandledException(object sender, UnhandledExceptionEventArgs ex)
    {
        if (ex.IsTerminating)
            logger.LogError("Unhandled Error: {ERROR}", ex.ExceptionObject);
        else
            logger.LogWarning("Unhandled Warning: {WARNING}", ex.ExceptionObject);

        if (!ex.IsTerminating) return;

        HasCrashed = true;

        var doClose = false;

        CrashedEventArgs arguments = new(ex.ExceptionObject as Exception);

        try
        {
            sink.InvokeCrashed(arguments);
            doClose = arguments.Close;
        }
        catch (Exception crashedException)
        {
            logger.LogError(crashedException, "Unable to invoke crashed arguments");
        }

        if (!doClose)
        {
            logger.LogCritical("This exception is fatal, press return to exit.");
            Console.ReadLine();
        }

        KillServer(false);
    }

    public void KillServer(bool restart)
    {
        HandleClosed();

        if (restart)
            Process.Start(GetExePath.Path());

        Process.GetCurrentProcess().Kill();
    }

    public void HandleClosed()
    {
        if (IsClosing)
            return;

        IsClosing = true;

        logger.LogError("Exiting server, please wait!");

        sink.InvokeInternalShutdown();

        if (!HasCrashed)
            sink.InvokeShutdown();

        logger.LogCritical("Successfully quit server.");
    }

    public void Set() => _signal.Set();
    public void Wait() => _signal.WaitOne();
}
