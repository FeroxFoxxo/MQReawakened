using Server.Base.Core.Abstractions;

namespace Server.Reawakened.Core.Models;

public class ServerConfig : IConfig
{
    public int RandomKeyLength { get; set; }
    public int PlayerCap { get; set; }
    public string[] DefaultProtocolTypeIgnore { get; set; }
    public int MaxCharacterCount { get; set; }

    public ServerConfig()
    {
        RandomKeyLength = 24;
        PlayerCap = 20;
        DefaultProtocolTypeIgnore = new[] { "ss", "Pp" };
        MaxCharacterCount = 3;
    }
}
