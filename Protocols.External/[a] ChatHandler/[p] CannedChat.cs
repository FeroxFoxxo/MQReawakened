using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Extensions;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Services;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Protocols.External._a__ChatHandler;

public class CannedChat : ExternalProtocol
{
    public override string ProtocolName => "ap";

    public ILogger<CannedChat> Logger { get; set; }
    public ServerRConfig Config { get; set; }
    public CannedChatDictionary CannedChatDict { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public DiscordHandler DiscordHandler { get; set; }

    public override void Run(string[] message)
    {
        var channelType = (CannedChatChannel)int.Parse(message[5]);
        var chatPhraseId = int.Parse(message[6]);
        var secondaryPhraseId = int.Parse(message[7]); // named 'specifics' in the client protocol/xml
        var itemId = int.Parse(message[8]);
        var recipientName = message[9];

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

        var sb = new SeparatedStringBuilder(' ');

        if (!string.IsNullOrEmpty(CannedChatDict.GetDialogById(chatPhraseId)))
            sb.Append(CannedChatDict.GetDialogById(chatPhraseId));

        if (!string.IsNullOrEmpty(CannedChatDict.GetDialogById(secondaryPhraseId)))
            sb.Append(CannedChatDict.GetDialogById(secondaryPhraseId));

        if (ItemCatalog.GetItemFromId(itemId) != null)
            sb.Append(ItemCatalog.GetItemFromId(itemId).ItemName);

        switch (channelType)
        {
            case CannedChatChannel.Speak:
                Player.Room.Chat(channelType, Player.Character.CharacterName, sb.ToString());

                // Sends a chat message to Discord
                DiscordHandler.SendMessage(Player.Character.CharacterName, sb.ToString());
                break;

            case CannedChatChannel.Group:
                foreach (
                    var client in
                        from client in Player.TempData.Group.GetMembers()
                        select client
                    )
                    client.Chat(channelType, Player.Character.CharacterName, sb.ToString());

                // Sends a chat message to Discord
                DiscordHandler.SendMessage("Group -> " + Player.Character.CharacterName, sb.ToString());
                break;

            case CannedChatChannel.Trade:
                if (Player.Room.LevelInfo.Type == LevelType.City)
                {
                    Player.Room.Chat(channelType, Player.Character.CharacterName, sb.ToString());

                    // Sends a chat message to Discord
                    DiscordHandler.SendMessage("Trade -> " + Player.Character.CharacterName, sb.ToString());
                }
                break;

            case CannedChatChannel.Tell:
            case CannedChatChannel.Reply:
                if (!string.IsNullOrEmpty(recipientName))
                {
                    var recipient = Player.PlayerContainer.GetPlayerByName(recipientName);

                    if (recipient != null && !recipient.Character.Blocked.Contains(Player.CharacterId))
                    {
                        Player.Chat(channelType, Player.Character.CharacterName, sb.ToString(), recipientName);

                        // Sends a chat message to Discord
                        DiscordHandler.SendMessage("PM -> From: " + Player.Character.CharacterName +
                            " To: " + recipientName, sb.ToString());
                    }
                }
                break;

            default:
                Logger.LogError("No chat handler found for {ChannelType} to '{Recipient}' for '{Message}'",
                    channelType, recipientName, sb.ToString());
                break;
        }
    }
}
