using Server.Base.Core.Abstractions;
using Server.Base.Network.Enums;

namespace Server.Base.Core.Configs;

public class InternalRwConfig : IRwConfig
{
    public string[] IgnoreProtocolType { get; set; }
    public NetworkType NetworkType { get; set; }
    public bool IsHttps { get; set; }
    public string ServerAddress { get; set; }
    public int Port { get; set; }
    public string[] UnhandledPackets { get; set; }
    public bool RestartOnCrash { get; set; }

    public string ServerName { get; set; }
    public string DiscordServerId { get; set; }
    
    public InternalRwConfig()
    {
        IgnoreProtocolType = [];
        UnhandledPackets = [];
        NetworkType = NetworkType.Unknown;
        ServerAddress = string.Empty;
        Port = 9339;
        RestartOnCrash = true;
        ServerName = "MQReawakened";
        IsHttps = false;
        DiscordServerId = string.Empty;

        if (int.TryParse(Environment.GetEnvironmentVariable("GAME_PORT"), out var p))
            Port = p;
    }

    public string GetHostName() => $"{ServerAddress}:{Port}";
    public string GetHostAddress() => (IsHttps ? "https" : "http") + $"://{ServerAddress}";
}
