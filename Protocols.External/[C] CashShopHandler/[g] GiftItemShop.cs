using A2m.Server;
using A2m.Server.Messages;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Models.System;
using Server.Reawakened.XMLs.Bundles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.Protocol;

namespace Protocols.External._C__CashShopHandler;
public class GiftItemShop : ExternalProtocol
{
    public override string ProtocolName => "Cg";

    public ItemCatalog ItemCatalog { get; set; }

    public PlayerHandler PlayerHandler { get; set; }

    public override void Run(string[] message)
    {
        var character = Player.Character;

        var otherPlayerName = message[6];
        var messageId = int.Parse(message[7]);
        var itemId = int.Parse(message[8]);
        var cardId = int.Parse(message[9]);
        var boxId = int.Parse(message[10]);

        var boxDescription = ItemCatalog.GetItemFromId(boxId);
        Player.RemoveBananas(boxDescription.RegularPrice);
        var itemDescription = ItemCatalog.GetItemFromId(itemId);
        Player.RemoveNCash(itemDescription.RegularPrice);
        var otherPlayer = PlayerHandler.PlayerList
            .FirstOrDefault(p => p.Character.Data.CharacterName == otherPlayerName);
        otherPlayer.Character.AddItem(itemDescription, 1);

        otherPlayer.SendUpdatedInventory(false);

        otherPlayer?.SendXt("Cg", 1, otherPlayerName, messageId, itemId, cardId, boxId);
    }
}
