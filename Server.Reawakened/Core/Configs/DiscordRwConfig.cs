using Server.Base.Core.Abstractions;

namespace Server.Reawakened.Core.Configs;
public class DiscordRwConfig : IRwConfig
{
    public string DiscordBotToken { get; set; }
    public ulong ChannelId { get; set; }

    public DiscordRwConfig()
    {
        DiscordBotToken = string.Empty;
        ChannelId = 0;
    }
}
