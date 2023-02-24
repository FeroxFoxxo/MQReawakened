using Server.Base.Core.Abstractions;
using Server.Base.Network.Enums;

namespace Server.Base.Core.Models;

public class InternalRwConfig : IRwConfig
{
    public string[] IgnoreProtocolType { get; set; }
    public NetworkType NetworkType { get; set; }

    public InternalRwConfig()
    {
        IgnoreProtocolType = Array.Empty<string>();
        NetworkType = NetworkType.Unknown;
    }
}
