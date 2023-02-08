using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Models;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;

namespace Server.Base.Core.Services;

public class ServerConsole : IService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly Dictionary<string, ConsoleCommand> _commands;
    private readonly Thread _consoleThread;
    private readonly ServerHandler _handler;
    private readonly ILogger<ServerConsole> _logger;
    private readonly TimerThread _timerThread;

    public ServerConsole(TimerThread timerThread, ServerHandler handler, EventSink sink, ILogger<ServerConsole> logger,
        IHostApplicationLifetime appLifetime)
    {
        _timerThread = timerThread;
        _handler = handler;
        _logger = logger;
        _appLifetime = appLifetime;

        _commands = new Dictionary<string, ConsoleCommand>();

        _consoleThread = new Thread(ConsoleLoopThread)
        {
            Name = "Console Thread"
        };
    }

    public void Initialize() => _appLifetime.ApplicationStarted.Register(RunConsoleListener);

    public void AddCommand(ConsoleCommand consoleCommand) => _commands.Add(consoleCommand.Name, consoleCommand);

    public void RunConsoleListener()
    {
        _logger.LogInformation("Setting Up Console Commands");

        AddCommand(new ConsoleCommand(
            "restart",
            "Sends a message to players informing them that the server is\n" +
            "           restarting, performs a forced save, then shuts down and\n" +
            "           restarts the server.",
            _ => _handler.KillServer(true)
        ));

        AddCommand(new ConsoleCommand(
            "shutdown",
            "Performs a forced save then shuts down the server.",
            _ => _handler.KillServer(false)
        ));

        AddCommand(new ConsoleCommand(
            "crash",
            "Forces an exception to be thrown.",
            _ => _timerThread.DelayCall(() => throw new Exception("Forced Crash"))
        ));

        DisplayHelp();

        _consoleThread.Start();
    }

    public void ConsoleLoopThread()
    {
        try
        {
            while (!_handler.IsClosing && !_handler.HasCrashed)
                ProcessCommand(Console.ReadLine());
        }
        catch (IOException)
        {
            // ignored
        }
    }

    private void ProcessCommand(string input)
    {
        if (_handler.IsClosing || _handler.HasCrashed)
            return;

        if (!string.IsNullOrEmpty(input))
        {
            var inputs = input.Trim().Split();
            var name = inputs.FirstOrDefault();

            if (name != null && _commands.TryGetValue(name, out var value))
            {
                value.CommandMethod(inputs);
                _logger.LogDebug("Successfully ran command '{Name}'", name);
                return;
            }
        }

        DisplayHelp();
    }

    private void DisplayHelp()
    {
        _logger.LogDebug("Commands:");

        foreach (var command in _commands.Values)
        {
            var padding = 8 - command.Name.Length;
            if (padding < 0) padding = 0;
            _logger.LogDebug("  {Name} - {Description}", command.Name.PadRight(padding), command.Description);
        }
    }
}
