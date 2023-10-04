using Server.Base.Core.Abstractions;
using Server.Base.Network.Enums;

namespace Server.Base.Core.Configs;

public class InternalRwConfig : IRwConfig
{
    public string[] IgnoreProtocolType { get; set; }
    public NetworkType NetworkType { get; set; }
    public string ServerAddress { get; set; }
    public int Port { get; }
    public string[] UnhandledPackets { get; set; }

    public InternalRwConfig()
    {
        IgnoreProtocolType = Array.Empty<string>();
        UnhandledPackets = Array.Empty<string>();
        NetworkType = NetworkType.Unknown;
        ServerAddress = string.Empty;
        Port = 9339;
    }

    public string GetHostName() => $"{ServerAddress}:{Port}";
    public string GetHostAddress() => $"http://{ServerAddress}";
}
