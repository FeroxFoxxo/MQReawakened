using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocols.External._C__CashShopHandler;
public class BuyItems : ExternalProtocol
{
    public override string ProtocolName => "Cb";

    public ItemCatalog ItemCatalog { get; set; }

    public override void Run(string[] message)
    {
        var character = Player.Character;

        //var vendorId = int.Parse(message[5]);
        //var vendorGoId = int.Parse(message[7]);
        var items = message[6].Split('|');
        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item)) continue;

            var args = item.Split(":");
            var itemId = int.Parse(args[0]);
            var amount = int.Parse(args[1]);
            character.AddItem(ItemCatalog.GetItemFromId(itemId), amount);
            var itemInfo = ItemCatalog.GetItemFromId(itemId);
            Player.AddNCash(-itemInfo.RegularPrice * amount);
        }
        Player.SendCashUpdate();
        Player.SendUpdatedInventory(false);

    }
}
