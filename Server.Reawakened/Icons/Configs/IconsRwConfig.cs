using Server.Base.Core.Abstractions;

namespace Web.Icons.Configs;

public class IconsRwConfig : IRwConfig
{
    public Dictionary<string, long> Configs { get; set; }

    public IconsRwConfig() => Configs = [];
}
