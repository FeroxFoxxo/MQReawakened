using Server.Base.Core.Abstractions;

namespace Server.Reawakened.Icons.Configs;

public class IconsRwConfig : IRwConfig
{
    public Dictionary<string, long> Configs { get; set; }

    public IconsRwConfig() => Configs = [];
}
