using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Events.Arguments;
using Server.Base.Core.Extensions;
using Server.Base.Core.Models;
using Server.Base.Network.Enums;
using Server.Base.Worlds;

namespace Server.Base.Core.Services;

public class OperationMode : IService
{
    private readonly EventSink _eventSink;
    private readonly ServerConsole _console;
    private readonly InternalRwConfig _config;
    private readonly ILogger<OperationMode> _logger;

    public OperationMode(EventSink eventSink, ServerConsole console, InternalRwConfig config, ILogger<OperationMode> logger)
    {
        _eventSink = eventSink;
        _console = console;
        _config = config;
        _logger = logger;
    }

    public void Initialize() => _eventSink.WorldLoad += CheckOperationalMode;

    private void CheckOperationalMode()
    {
        _console.AddCommand("changeOperationalMode", "Changes the mode the game is set to (client/server/both)",
            NetworkType.Unknown | NetworkType.Server | NetworkType.Client, _ => AskForChange());

        _console.AddCommand("changeServerAddress", "Changes the server address the client will connect to.",
            NetworkType.Client, _ => SetServerAddress(), true);

        switch (_config.NetworkType)
        {
            case NetworkType.Unknown:
                AskForChange();
                break;
            case NetworkType.Client when _config.StrictNetworkCheck():
            {
                if (string.IsNullOrEmpty(_config.ServerAddress))
                    SetServerAddress();
            
                _logger.LogInformation("Server address set to: {Address}", _config.ServerAddress);
                break;
            }
        }
    }

    private void AskForChange()
    {
        _logger.LogError("Please note: multi-player mode is not implemented yet. Please use the 'changeOperationalMode' to change back to single-player.");

        if (_logger.Ask("Would you like to use single-player mode?", true))
        {
            _config.NetworkType = NetworkType.Client | NetworkType.Server;
            return;
        }

        if (_logger.Ask("Are you setting this app as a client to connect to a server, rather than being the server itself?", true))
        {
            _config.NetworkType = NetworkType.Client;

            _logger.LogInformation("What is the address of the server that you are trying to connect to?");

            var serverAddress = Console.ReadLine();

            _config.ServerAddress = serverAddress;

            return;
        }

        _config.NetworkType = NetworkType.Server;
    }

    private void SetServerAddress()
    {
        while (true)
        {
            _logger.LogInformation("What is the address of the server that you are trying to connect to?");

            var serverAddress = Console.ReadLine();

            _config.ServerAddress = serverAddress;

            if (!string.IsNullOrEmpty(_config.ServerAddress))
                return;

            _logger.LogError("Server address cannot be empty!");
        }
    }
}
