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

        if (config.NetworkType == NetworkType.Unknown)
            ChangeNetworkType();

        logger.LogInformation("Playing as: {Mode} connected to {Address}", config.NetworkType,
            config.GetHostAddress());
    }

    private void ChangeNetworkType()
    {
        AskForChange();
        eventSink.InvokeChangedOperationalMode();
    }

    private void AskForChange()
    {
        if (logger.Ask(
                "Are you wanting to play the game, rather than host one?",
                true
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
            .Replace("\\r\\n", string.Empty).Replace("\\n", string.Empty).Trim();

        return !IPAddress.TryParse(externalIpString, out var ipAddress) ? null : ipAddress;
    }

    private void SetServerAddress(string question)
    {
        while (true)
        {
            logger.LogInformation("{Question}", question);

            var serverAddress = Console.ReadLine();

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
