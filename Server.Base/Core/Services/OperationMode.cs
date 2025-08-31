using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Configs;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Network.Enums;
using System.Net;

namespace Server.Base.Core.Services;

public class OperationMode(EventSink eventSink, ServerConsole console, InternalRwConfig config,
    ILogger<OperationMode> logger) : IService
{
    public void Initialize() => eventSink.WorldLoad += CheckOperationalMode;

    private void CheckOperationalMode()
    {
        console.AddCommand("changeOperationalMode", "Changes the mode the game is set to (client/server/both)",
            NetworkType.Unknown | NetworkType.Server | NetworkType.Client, _ => ChangeNetworkType());

        var networkTypeEnv = Environment.GetEnvironmentVariable("NETWORK_TYPE");

        if (!string.IsNullOrWhiteSpace(networkTypeEnv) && Enum.TryParse<NetworkType>(networkTypeEnv, true, out var parsed))
            config.NetworkType = parsed;

        var serverAddressEnv = Environment.GetEnvironmentVariable("SERVER_ADDRESS");
        
        if (!string.IsNullOrWhiteSpace(serverAddressEnv))
            config.ServerAddress = serverAddressEnv;

        if (int.TryParse(Environment.GetEnvironmentVariable("GAME_PORT"), out var p))
            config.Port = p;

        if (config.NetworkType == NetworkType.Unknown)
            ChangeNetworkType();

        logger.LogInformation("Playing as: '{Mode}' connected to '{Address}'", config.NetworkType.ToString().ToLower(),
            config.GetHostAddress());
    }

    private void ChangeNetworkType()
    {
        AskForChange();
        eventSink.InvokeChangedOperationalMode();
    }

    private void AskForChange()
    {
        if (EnvironmentExt.IsContainer())
        {
            config.NetworkType = NetworkType.Server;

            if (string.IsNullOrWhiteSpace(config.ServerAddress))
            {
                var envAddr = Environment.GetEnvironmentVariable("SERVER_ADDRESS");
                config.ServerAddress = string.IsNullOrWhiteSpace(envAddr) ? "localhost" : envAddr;
            }

            logger.LogInformation("Set IP Address to: {Address}", config.ServerAddress);
            return;
        }

        // This should not be changed as the default values should be set to run a server via docker.
        if (logger.Ask(
                "Are you wanting to play the game, rather than host one?",
                false
            ))
        {
            if (logger.Ask(
                    "Would you like to connect to an online server, rather than be put into single-player mode?",
                    true
                ))
            {
                config.NetworkType = NetworkType.Client;
                SetServerAddress("What is the address of the server that you are trying to connect to?");
            }
            else
            {
                config.NetworkType = NetworkType.Client | NetworkType.Server;
                config.ServerAddress = "localhost";
            }
        }
        else
        {
            config.NetworkType = NetworkType.Server;

            if (logger.Ask(
                    "Would you like to automatically detect your external IP, rather than set it manually?",
                    true
                ))
            {
                var externalIpTask = GetExternalIpAddress();
                GetExternalIpAddress().Wait();
                var externalIpString = externalIpTask.Result ?? IPAddress.Loopback;
                config.ServerAddress = externalIpString.ToString();
            }
            else
            {
                SetServerAddress("What is the address of the server that you are trying to host?");
            }

            logger.LogInformation("Set IP Address to: {Address}", config.ServerAddress);
        }
    }

    public static async Task<IPAddress> GetExternalIpAddress()
    {
        var externalIpString = (await new HttpClient().GetStringAsync("http://icanhazip.com"))
            .Replace("\\r\\n", string.Empty).Replace("\\n", string.Empty)?.Trim();

        return !IPAddress.TryParse(externalIpString, out var ipAddress) ? null : ipAddress;
    }

    private void SetServerAddress(string question)
    {
        while (true)
        {
            logger.LogInformation("{Question}", question);

            var serverAddress = ConsoleExt.ReadOrEnv("SERVER_ADDRESS", logger);

            config.ServerAddress = serverAddress;

            if (!string.IsNullOrEmpty(config.ServerAddress))
            {
                if (!serverAddress!.Contains("http"))
                    break;

                logger.LogError("Server address cannot be a url, just a domain or ip address.");
                continue;
            }

            logger.LogError("Server address cannot be empty!");
        }
    }
}
