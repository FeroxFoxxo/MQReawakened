﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Configs;
using Server.Base.Core.Extensions;
using Server.Base.Core.Models;
using Server.Base.Network.Enums;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Base.Worlds;
using static Server.Base.Core.Models.ConsoleCommand;

namespace Server.Base.Core.Services;

public class ServerConsole : IService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly Dictionary<string, ConsoleCommand> _commands;
    private readonly InternalRConfig _rConfig;
    private readonly InternalRwConfig _rwConfig;
    private readonly Thread _consoleThread;
    private readonly ServerHandler _handler;
    private readonly ILogger<ServerConsole> _logger;
    private readonly TimerThread _timerThread;
    private readonly World _world;

    public ServerConsole(TimerThread timerThread, ServerHandler handler, ILogger<ServerConsole> logger,
        IHostApplicationLifetime appLifetime, InternalRConfig rConfig, InternalRwConfig rwConfig, World world)
    {
        _timerThread = timerThread;
        _handler = handler;
        _logger = logger;
        _appLifetime = appLifetime;
        _rConfig = rConfig;
        _rwConfig = rwConfig;
        _world = world;

        _commands = new Dictionary<string, ConsoleCommand>();

        _consoleThread = new Thread(ConsoleLoopThread)
        {
            Name = "Console Thread"
        };
    }

    public void Initialize() => _appLifetime.ApplicationStarted.Register(RunConsoleListener);

    public void RunConsoleListener()
    {
        _logger.LogDebug("Setting up console commands");

        AddCommand(
            "restart",
            "Sends a message to players informing them that the server is\n" +
            "           restarting, performs a forced save, then shuts down and\n" +
            "           restarts the server.",
            NetworkType.Server,
            _ => _handler.KillServer(true)
        );

        AddCommand(
            "shutdown",
            "Performs a forced save then shuts down the app.",
            NetworkType.Server | NetworkType.Client,
            _ => _handler.KillServer(false)
        );

        AddCommand(
            "save",
            "Forces a save.",
            NetworkType.Server,
            _ => _world.Save(true, false)
        );

        AddCommand(
            "crash",
            "Forces an exception to be thrown.",
            NetworkType.Server,
            _ => _timerThread.DelayCall(() => throw new Exception("Forced Crash"))
        );

        DisplayHelp();

        _consoleThread.Start();
    }

    public void AddCommand(string name, string description, NetworkType networkType, RunConsoleCommand commandMethod, bool strictCheck = false) =>
        _commands.Add(name, new ConsoleCommand
        {
            CommandMethod = commandMethod,
            Description = description,
            NetworkType = networkType,
            Name = name,
            StrictCheck = strictCheck
        });

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
                _logger.LogInformation("Successfully ran command '{Name}'", name);
            }
        }

        DisplayHelp();
    }

    private static IEnumerable<NetworkType> GetFlags(NetworkType networkType) =>
        Enum.GetValues(typeof(NetworkType))
            .Cast<NetworkType>()
            .Where(v => networkType.HasFlag(v))
            .ToArray();

    public void DisplayHelp()
    {
        _logger.LogInformation("Commands:");

        var commands = _commands.Values
            .Where(x => GetFlags(x.NetworkType)
                .Any(y => GetFlags(_rwConfig.NetworkType)
                    .Any(z => z == y)
                )
            )
            .Where(x => !x.StrictCheck || _rwConfig.StrictNetworkCheck())
            .OrderBy(x => x.Name)
            .ToArray();

        if (commands.Any())
            foreach (var command in commands)
            {
                var padding = _rConfig.CommandPadding - command.Name.Length;
                if (padding < 0) padding = 0;
                _logger.LogInformation("  {Name} - {Description}", command.Name.PadRight(padding), command.Description);
            }
        else
            _logger.LogError("Could not find any commands! Are you sure you are running the correct operational type? " +
                             "Current flags: {Flags}", _rwConfig.NetworkType);
    }
}
