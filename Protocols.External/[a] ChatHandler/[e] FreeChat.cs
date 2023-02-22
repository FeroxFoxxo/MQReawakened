using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Chat.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._a__ChatHandler;

public class FreeChat : ExternalProtocol
{
    public override string ProtocolName => "ae";

    public ILogger<ChatCommands> Logger { get; set; }
    public ChatCommands ChatCommands { get; set; }
    public ServerStaticConfig Config { get; set; }

    public override void Run(string[] message)
    {
        var channelType = (CannedChatChannel)Convert.ToInt32(message[5]);
        var chatMessage = message[6];
        var recipientName = message[7];

        if (chatMessage.StartsWith(Config.ChatCommandStart))
        {
            var args = chatMessage.Contains(' ') ? chatMessage[1..].Split(' ') : new[] { chatMessage[1..] };

            ChatCommands.RunCommand(NetState, args);
        }
        else if (channelType == CannedChatChannel.Speak)
        {
            var player = NetState.Get<Player>();
            var character = player.GetCurrentCharacter();
            player.CurrentRoom.Chat(channelType, character.Data.CharacterName, chatMessage);
        }
        else
        {
            Logger.LogError("No chat handler found for {ChannelType} to '{Recipient}' for '{Message}'",
                channelType, recipientName, chatMessage);
        }
    }
}
