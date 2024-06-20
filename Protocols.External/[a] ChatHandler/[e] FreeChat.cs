using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Services;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;

namespace Protocols.External._a__ChatHandler;

public class FreeChat : ExternalProtocol
{
    public override string ProtocolName => "ae";

    public ILogger<FreeChat> Logger { get; set; }
    public ServerRConfig Config { get; set; }
    public DiscordHandler DiscordHandler { get; set; }

    public override void Run(string[] message)
    {
        var channelType = (CannedChatChannel)Convert.ToInt32(message[5]);
        var chatMessage = message[6];
        var recipientName = message[7];

        if (channelType == CannedChatChannel.Speak)
        {
            Player.Room.Chat(channelType, Player.Character.CharacterName, chatMessage);

            // Sends a chat message to Discord
            DiscordHandler.SendMessage(Player.Character.CharacterName, chatMessage);
        }
        else if (channelType == CannedChatChannel.Group)
        {
            foreach (
                var client in
                    from client in Player.TempData.Group.GetMembers()
                    select client
                )
                client.Chat(channelType, Player.Character.CharacterName, chatMessage);
        }
        else if (channelType == CannedChatChannel.Trade)
        {
            if (Player.Room.LevelInfo.Type == LevelType.City)
                Player.Room.Chat(channelType, Player.Character.CharacterName, chatMessage);
        }
        else if (channelType is CannedChatChannel.Tell or CannedChatChannel.Reply)
        {
            if (!string.IsNullOrEmpty(recipientName))
            {
                var recipient = Player.PlayerContainer.GetPlayerByName(recipientName);

                if (recipient != null && !recipient.Character.Blocked.Contains(Player.CharacterId))
                    Player.Chat(channelType, Player.Character.CharacterName, chatMessage, recipientName);
            }
        }
        else
        {
            Logger.LogError("No chat handler found for {ChannelType} to '{Recipient}' for '{Message}'",
                channelType, recipientName, chatMessage);
        }
    }
}
