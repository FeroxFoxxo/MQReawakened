using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._C__CashShopHandler;

public class GiftItemShop : ExternalProtocol
{
    public override string ProtocolName => "Cg";

    public ItemCatalog ItemCatalog { get; set; }
    public PlayerHandler PlayerHandler { get; set; }
    public ILogger<GiftItemShop> Logger { get; set; }

    public override void Run(string[] message)
    {
        var cashShop = (Cashshop)int.Parse(message[5]);

        if (cashShop != Cashshop.CashShop)
        {
            Logger.LogWarning("Unknown cashshop of type {Type}!", cashShop);
            return;
        }

        var friendName = message[6];
        var _1 = message[7]; // messageDesc
        var itemId = int.Parse(message[8]);
        var _2 = int.Parse(message[9]); // backgroundId
        var packageId = int.Parse(message[10]);

        var package = ItemCatalog.GetItemFromId(packageId);
        Player.RemoveBananas(package.RegularPrice);

        var item = ItemCatalog.GetItemFromId(itemId);
        Player.RemoveNCash(item.RegularPrice);

        var friend = PlayerHandler.GetPlayerByName(friendName);

        friend.Character.AddItem(item, 1);
        friend.SendUpdatedInventory(false);

        Logger.LogError("Gifting is not implemented yet!");
    }
}
