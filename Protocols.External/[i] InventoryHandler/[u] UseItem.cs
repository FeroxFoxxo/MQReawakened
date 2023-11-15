using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Protocols.External._i__InventoryHandler;

public class UseItem : ExternalProtocol
{
    public override string ProtocolName => "iu";

    public ILogger<UseItem> Logger { get; set; }

    public VendorCatalog VendorCatalog { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public RecipeCatalogInt RecipeCatalogInt { get; set; }
    public override void Run(string[] message)
    {
        var character = Player.Character;

        var itemId = int.Parse(message[5]);

        var item = ItemCatalog.GetItemFromId(itemId);

        int recipeParentId;

        if (item == null)
        {
            Logger.LogError("Could not find item with id {ItemId}", itemId);
            return;
        }

        character.RemoveItem(item, 1);

        switch (item.SubCategoryId)
        {
            case ItemSubCategory.Alchemy:
                recipeParentId = item.RecipeParentItemID;
                Player.GrantRecipe(itemId, recipeParentId, RecipeCatalogInt, Logger);
                break;
            case ItemSubCategory.Tailoring:
                recipeParentId = item.RecipeParentItemID;
                Player.GrantRecipe(itemId, recipeParentId, RecipeCatalogInt, Logger);
                break;
            case ItemSubCategory.SuperPack:

                foreach (var pair in VendorCatalog.GetSuperPacksItemQuantityMap(itemId))
                {
                    var packItem = ItemCatalog.GetItemFromId(pair.Key);

                    if (packItem == null)
                        continue;

                    character.AddItem(packItem, pair.Value);
                }

                break;
            default:
                Logger.LogWarning("Could not find use for item {ItemId}, type {ItemType}.",
                    itemId, item.SubCategoryId);
                break;
        }

        Player.SendUpdatedInventory(false);
    }

}
