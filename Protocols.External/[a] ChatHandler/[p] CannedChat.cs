using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Protocols.External._a__ChatHandler;

public class CannedChat : ExternalProtocol
{
    public override string ProtocolName => "ap";

    public ILogger<CannedChat> Logger { get; set; }
    public ServerRConfig Config { get; set; }
    public CannedChatDictionary CannedChatDict { get; set; }

    public override void Run(string[] message)
    {
        var channelType = (CannedChatChannel)int.Parse(message[5]);
        var chatPhraseId = int.Parse(message[6]);
        _ = message[7]; // recipientName

        var chatPhrase = CannedChatDict.GetDialogById(chatPhraseId);

        Player.Room.Chat(channelType, Player.CharacterName, chatPhrase);
    }
}
