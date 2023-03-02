using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Models;
using Server.Base.Network.Enums;
using System.Net;

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
        
        if (_config.NetworkType == NetworkType.Unknown)
            AskForChange();
    }

    private void AskForChange()
    {
        _logger.LogError("Please note: multi-player mode is not implemented yet. Please use the 'changeOperationalMode' to change back to single-player.");

        if (_logger.Ask("Would you like to use single-player mode?", true))
        {
            _config.NetworkType = NetworkType.Client | NetworkType.Server;
            _eventSink.InvokeChangedOperationalMode();
            return;
        }

        if (_logger.Ask("Are you setting this app as a client to connect to a server, rather than being the server itself?", true))
        {
            _config.NetworkType = NetworkType.Client;
            SetServerAddress("What is the address of the server that you are trying to connect to?");
            return;
        }

        _config.NetworkType = NetworkType.Server;
        
        if (_logger.Ask("Would you like to manually set your server address, rather than choosing your external IP?", true))
        {
            SetServerAddress("What is the address of the server that you are trying to host?");
        }
        else
        {
            var externalIpTask = GetExternalIpAddress();
            GetExternalIpAddress().Wait();
            var externalIpString = externalIpTask.Result ?? IPAddress.Loopback;
            _config.ServerAddress = externalIpString.ToString();
        }
        _logger.LogInformation("Set IP Address to: {Address}", _config.ServerAddress);
    }

    public static async Task<IPAddress> GetExternalIpAddress()
    {
        var externalIpString = (await new HttpClient().GetStringAsync("http://icanhazip.com"))
            .Replace("\\r\\n", "").Replace("\\n", "").Trim();

        return !IPAddress.TryParse(externalIpString, out var ipAddress) ? null : ipAddress;
    }

    private void SetServerAddress(string question)
    {
        while (true)
        {
            _logger.LogInformation("{Question}", question);

            var serverAddress = Console.ReadLine();

            _config.ServerAddress = serverAddress;

            if (!string.IsNullOrEmpty(_config.ServerAddress))
            {
                if (!serverAddress!.Contains("http"))
                    break;

                _logger.LogError("Server address cannot be a url, just a domain or ip address.");
                continue;
            }
            _logger.LogError("Server address cannot be empty!");
        }

        _eventSink.InvokeChangedOperationalMode();
    }
}
