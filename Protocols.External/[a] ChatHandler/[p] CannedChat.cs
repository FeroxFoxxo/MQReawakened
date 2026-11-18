using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Protocols.External._a__ChatHandler;

public class CannedChat : ExternalProtocol
{
    public override string ProtocolName => "ap";

    public ILogger<CannedChatDictionary> Logger { get; set; }
    public ServerRConfig Config { get; set; }
    public CannedChatDictionary CannedChatDict { get; set; }
    public ItemCatalog ItemCatalog { get; set; }

    public override void Run(string[] message)
    {
        var channelType = (CannedChatChannel)int.Parse(message[5]);
        var chatPhraseId = int.Parse(message[6]);
        var secondaryPhraseId = int.Parse(message[7]); // named 'specifics' in the client protocol/xml
        var itemId = int.Parse(message[8]);
        var recipientName = message[9];

        Player.SendCannedChat(channelType, chatPhraseId, secondaryPhraseId, itemId, recipientName, ItemCatalog, Logger, Config, CannedChatDict);
    }
}
