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
    public ItemCatalog ItemCatalog { get; set; }

    public override void Run(string[] message)
    {
        var channelType = int.Parse(message[5]);
        var chatPhraseId = int.Parse(message[6]);
        var secondaryPhraseId = int.Parse(message[7]); // named 'specifics' in the client protocol/xml
        var itemId = int.Parse(message[8]);

        var secondaryPhrase = CannedChatDict.GetDialogById(secondaryPhraseId);
        var itemName = ItemCatalog.GetItemFromId(itemId).ItemName;

        Player.SendXt("ap", channelType, Player.CharacterName, chatPhraseId, 
            string.IsNullOrEmpty(secondaryPhrase) ? 0 : secondaryPhraseId, string.IsNullOrEmpty(itemName) ? 0 : itemId);
    }
}
