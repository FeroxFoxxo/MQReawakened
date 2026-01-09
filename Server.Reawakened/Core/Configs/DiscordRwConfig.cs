using Server.Base.Core.Abstractions;

namespace Server.Reawakened.Core.Configs;
public class DiscordRwConfig : IRwConfig
{
    public string DiscordBotToken { get; set; }
    public ulong RoomChannelId { get; set; }
    public ulong GroupChannelId { get; set; }
    public ulong PrivateMessagesChannelId { get; set; }
    public ulong GlobalChannelId { get; set; }
    public ulong ReportsChannelId { get; set; }

    public DiscordRwConfig()
    {
        DiscordBotToken = string.Empty;
        RoomChannelId = 0;
        GroupChannelId = 0;
        PrivateMessagesChannelId = 0;
        GlobalChannelId = 0;
        ReportsChannelId = 0;
    }
}
