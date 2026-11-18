using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Protocols.External._a__ChatHandler;

public class SendFavorite : ExternalProtocol
{
    public override string ProtocolName => "af";

    public ILogger<CannedChatDictionary> Logger { get; set; }
    public ServerRConfig Config { get; set; }
    public CannedChatDictionary CannedChatDict { get; set; }
    public ItemCatalog ItemCatalog { get; set; }

    public override void Run(string[] message)
    {
        var channelType = (CannedChatChannel)int.Parse(message[5]);
        var slotId = int.Parse(message[6]);
        var recipientName = message[7];

        if (!Player.Character.ChatFavorites.TryGetValue(slotId, out var favorite))
        {
            Logger.LogInformation("Couldn't find favorite slot of id = {Id}", slotId);
            return;
        }
        
        Player.SendCannedChat(channelType, favorite.SentenceTextId, favorite.SpecificTextId, 0, recipientName, ItemCatalog, Logger, Config, CannedChatDict);
    }
}
