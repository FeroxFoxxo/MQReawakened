using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Protocols.External._i__InventoryHandler;

public class UseItem : ExternalProtocol
{
    public override string ProtocolName => "iu";

    public VendorCatalog VendorCatalog { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public InternalRecipe RecipeCatalog { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public TimerThread TimerThread { get; set; }
    public ILogger<PlayerStatus> Logger { get; set; }

    public override void Run(string[] message)
    {
        var itemId = int.Parse(message[5]);
        var usedItem = ItemCatalog.GetItemFromId(itemId);

        if (usedItem == null)
        {
            Logger.LogError("Could not find item with id {ItemId}", itemId);
            return;
        }

        Player.RemoveItem(usedItem, 1);

        var position = Player.TempData.Position;
        var direction = Player.TempData.Direction;

        var subCategoryId = usedItem.SubCategoryId;

        // Causes compile error due to missing removeFromHotbar variable
        //if (item.UniqueInInventory)
        //    removeFromHotbar = false;

        switch (subCategoryId)
        {
            case ItemSubCategory.Bomb:
                Player.HandleDrop(ServerRConfig, TimerThread, Logger, usedItem, position, direction);
                break;
            case ItemSubCategory.Nut:
            case ItemSubCategory.Usable:
            case ItemSubCategory.Liquid:
            case ItemSubCategory.Elixir:
            case ItemSubCategory.Potion:
                HandleConsumable(usedItem);
                break;
            case ItemSubCategory.Tailoring:
            case ItemSubCategory.Alchemy:
                var recipe = RecipeCatalog.GetRecipeById(itemId);

                Player.Character.Data.RecipeList.RecipeList.Add(recipe);

                Player.SendXt("cz", recipe);
                break;
            case ItemSubCategory.SuperPack:
                foreach (var pair in VendorCatalog.GetSuperPacksItemQuantityMap(itemId))
                {
                    var packItem = ItemCatalog.GetItemFromId(pair.Key);

                    if (packItem == null)
                        continue;

                    Player.AddItem(packItem, pair.Value);
                }
                break;
            default:
                Logger.LogWarning("Could not find use for item {ItemId}, type {ItemType}.",
                    itemId, usedItem.SubCategoryId);
                return;
        }

        Player.SendUpdatedInventory(false);
    }

    private void HandleConsumable(ItemDescription usedItem)
    {
        var removeFromHotbar = true;

        if (usedItem.InventoryCategoryID is
            ItemFilterCategory.WeaponAndAbilities or
            ItemFilterCategory.Pets)
            removeFromHotbar = false;

        if (removeFromHotbar)
            RemoveFromHotbar(Player.Character, usedItem);
    }

    private void RemoveFromHotbar(CharacterModel character, ItemDescription item)
    {
        character.Data.Inventory.Items[item.ItemId].Count--;

        if (character.Data.Inventory.Items[item.ItemId].Count <= 0)
            if (character.Data.Inventory.Items[item.ItemId] != null)
                character.Data.Inventory.Items.Remove(item.ItemId);

        Player.SendUpdatedInventory(false);
    }
}
