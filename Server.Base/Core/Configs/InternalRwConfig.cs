using Server.Base.Core.Abstractions;
using Server.Base.Network.Enums;

namespace Server.Base.Core.Configs;

public class InternalRwConfig : IRwConfig
{
    public string[] IgnoreProtocolType { get; set; }
    public NetworkType NetworkType { get; set; }
    public string ServerAddress { get; set; }
    public int Port { get; set; }
    public string[] UnhandledPackets { get; set; }
    public bool IndentSaves { get; set; }
    public bool RestartOnCrash { get; set; }

    public string ServerName { get; }

    public InternalRwConfig()
    {
        IgnoreProtocolType = [];
        UnhandledPackets = [];
        NetworkType = NetworkType.Unknown;
        ServerAddress = string.Empty;
        Port = 9339;
        IndentSaves = true;
        RestartOnCrash = true;
        ServerName = "MQReawakened";
    }

    public string GetHostName() => $"{ServerAddress}:{Port}";
    public string GetHostAddress() => $"http://{ServerAddress}";
}
