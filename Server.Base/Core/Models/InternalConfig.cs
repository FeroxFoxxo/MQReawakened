using Server.Base.Core.Abstractions;

namespace Server.Base.Core.Models;

public class InternalConfig : IConfig
{
    public string[] IgnoreProtocolType { get; set; }

    public InternalConfig() =>
        IgnoreProtocolType = Array.Empty<string>();
}
