using Server.Base.Core.Abstractions;
using Server.Base.Network.Enums;

namespace Server.Base.Core.Models;

public class InternalConfig : IConfig
{
    public string[] IgnoreProtocolType { get; set; }
    public NetworkType NetworkType { get; set; }

    public InternalConfig()
    {
        IgnoreProtocolType = Array.Empty<string>();
        NetworkType = NetworkType.Both;
    }
}
