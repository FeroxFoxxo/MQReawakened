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

        var phrase = CannedChatDict.GetDialogById(chatPhraseId);
        var secondaryPhrase = string.Empty;
        var item = string.Empty;

        if (string.IsNullOrEmpty(CannedChatDict.GetDialogById(secondaryPhraseId)))
            secondaryPhraseId = 0;
        else
            secondaryPhrase = CannedChatDict.GetDialogById(secondaryPhraseId);

        if (ItemCatalog.GetItemFromId(itemId) == null)
            itemId = 0;
        else
            item = ItemCatalog.GetItemFromId(itemId).ItemName;

        Player.Room.Chat((CannedChatChannel)channelType, Player.CharacterName, phrase +
            (secondaryPhraseId == 0 ? "" : secondaryPhrase) + (itemId == 0 ? "" : item));
    }
}
