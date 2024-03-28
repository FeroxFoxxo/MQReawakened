using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Chat.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;

namespace Protocols.External._a__ChatHandler;

public class FreeChat : ExternalProtocol
{
    public override string ProtocolName => "ae";

    public ILogger<ChatCommands> Logger { get; set; }
    public ChatCommands ChatCommands { get; set; }
    public ServerRConfig Config { get; set; }

    public override void Run(string[] message)
    {
        var channelType = (CannedChatChannel)Convert.ToInt32(message[5]);
        var chatMessage = message[6];
        var recipientName = message[7];

        if (chatMessage.StartsWith(Config.ChatCommandStart))
        {
            var args = chatMessage.Contains(' ') ? chatMessage[1..].Split(' ') : [chatMessage[1..]];

            ChatCommands.RunCommand(Player, args);
        }
        else if (channelType == CannedChatChannel.Speak)
        {
            var character = Player.Character;
            Player.Room.Chat(channelType, character.Data.CharacterName, chatMessage);
        }
        else if (channelType == CannedChatChannel.Group)
        {
            var character = Player.Character;

            foreach (
                var client in
                    from client in Player.TempData.Group.GetMembers()
                    select client
                )
                client.Chat(channelType, character.Data.CharacterName, chatMessage);
        }
        else if (channelType == CannedChatChannel.Trade)
        {
            var character = Player.Character;

            if (Player.Room.LevelInfo.Type == LevelType.City)
                Player.Room.Chat(channelType, character.Data.CharacterName, chatMessage);
        }
        else if (channelType is CannedChatChannel.Tell or CannedChatChannel.Reply)
        {
            var character = Player.Character;

            if (!string.IsNullOrEmpty(recipientName))
            {
                var recipient = Player.PlayerContainer.GetPlayerByName(recipientName);

                if (recipient != null && !recipient.Character.Data.Blocked.Contains(Player.CharacterId))
                    Player.Chat(channelType, character.Data.CharacterName, chatMessage, recipientName);
            }
        }
        else
        {
            Logger.LogError("No chat handler found for {ChannelType} to '{Recipient}' for '{Message}'",
                channelType, recipientName, chatMessage);
        }
    }
}
