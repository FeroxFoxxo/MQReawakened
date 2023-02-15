using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._i__InventoryHandler;

public class UseItem : ExternalProtocol
{
    public override string ProtocolName => "iu";

    public ILogger<UseItem> Logger { get; set; }

    public VendorCatalog VendorCatalog { get; set; }
    public ItemCatalog ItemCatalog { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();
        var character = player.GetCurrentCharacter();

        var itemId = Convert.ToInt32(message[5]);

        var item = ItemCatalog.GetItemFromId(itemId);

        if (item == null)
        {
            Logger.LogError("Could not find item with id {ItemId}", itemId);
            return;
        }

        switch (item.SubCategoryId)
        {
            case ItemSubCategory.SuperPack:
                character.RemoveItem(item.ItemId, 1);

                foreach (var pair in VendorCatalog.GetSuperPacksItemQuantityMap(itemId))
                {
                    var packItem = ItemCatalog.GetItemFromId(pair.Key);

                    if (packItem == null)
                        continue;

                    character.AddItem(packItem, pair.Value);
                }

                character.SendUpdatedInventory(NetState, false);
                break;
            default:
                Logger.LogWarning("Could not find use for item {ItemId}, type {ItemType}.",
                    itemId, item.SubCategoryId);
                break;
        }
    }
}
