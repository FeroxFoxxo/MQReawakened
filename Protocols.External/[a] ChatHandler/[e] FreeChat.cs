using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Extensions;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Services;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

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

        if (!Config.Chat)
        {
            Player.SendWarningMessage("chat");
            return;
        }

        if (Player.Account.IsMuted())
        {
            Player.Chat(CannedChatChannel.Tell, "Console", "You are muted" + Player.Account.FormatMuteTime() + ".");
            return;
        }

        switch (channelType)
        {
            case CannedChatChannel.Speak:
                Player.Room.Chat(channelType, Player.Character.CharacterName, chatMessage);

                // Sends a chat message to Discord
                DiscordHandler.SendMessage(Player.Character.CharacterName, chatMessage);
                break;

            case CannedChatChannel.Group:
                foreach (
                    var client in
                        from client in Player.TempData.Group.GetMembers()
                        select client
                    )
                    client.Chat(channelType, Player.Character.CharacterName, chatMessage);

                // Sends a chat message to Discord
                DiscordHandler.SendMessage("Group -> " + Player.Character.CharacterName, chatMessage);
                break;

            case CannedChatChannel.Trade:
                if (Player.Room.LevelInfo.Type == LevelType.City)
                {
                    Player.Room.Chat(channelType, Player.Character.CharacterName, chatMessage);

                    // Sends a chat message to Discord
                    DiscordHandler.SendMessage("Trade -> " + Player.Character.CharacterName, chatMessage);
                }
                break;

            case CannedChatChannel.Tell:
            case CannedChatChannel.Reply:
                if (!string.IsNullOrEmpty(recipientName))
                {
                    var recipient = Player.PlayerContainer.GetPlayerByName(recipientName);

                    if (recipient != null && !recipient.Character.Blocked.Contains(Player.CharacterId))
                    {
                        Player.Chat(channelType, Player.Character.CharacterName, chatMessage, recipientName);

                        // Sends a chat message to Discord
                        DiscordHandler.SendMessage("PM -> From: " + Player.Character.CharacterName +
                            " To: " + recipientName, chatMessage);
                    }
                }
                break;

            default:
                Logger.LogError("No chat handler found for {ChannelType} to '{Recipient}' for '{Message}'",
                    channelType, recipientName, chatMessage);
                break;
        }
    }
}
