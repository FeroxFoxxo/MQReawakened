using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.Rooms.Models.Planes;
using static LeaderBoardTopScoresJson;

namespace Protocols.External._i__InventoryHandler;

public class UseItem : ExternalProtocol
{
    public override string ProtocolName => "iu";

    public VendorCatalog VendorCatalog { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public InternalRecipe RecipeCatalog { get; set; }
    public ItemRConfig ItemRConfig { get; set; }
    public TimerThread TimerThread { get; set; }
    public ILogger<PlayerStatus> Logger { get; set; }
    public InternalAchievement InternalAchievement { get; set; }

    public override void Run(string[] message)
    {
        var itemId = int.Parse(message[5]);
        var usedItem = ItemCatalog.GetItemFromId(itemId);

        if (usedItem == null)
        {
            Logger.LogError("Could not find item with id {ItemId}", itemId);
            return;
        }

        var position = Player.TempData.Position;
        var direction = Player.TempData.Direction;

        var subCategoryId = usedItem.SubCategoryId;

        switch (subCategoryId)
        {
            case ItemSubCategory.Bomb:
                HandleBomb(usedItem, position, direction);
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
                HandleRecipe(usedItem);
                break;
            case ItemSubCategory.SuperPack:
                HandleSuperPack(usedItem);
                break;
            case ItemSubCategory.Pets:
                HandlePet(usedItem);
                break;
            default:
                Logger.LogWarning("Could not find use for item {ItemId}, type {ItemType}.",
                    itemId, usedItem.SubCategoryId);
                return;
        }

        Player.SendUpdatedInventory();
    }

    private void HandlePet(ItemDescription usedItem)
    {
        var itemModel = Player.Character.Data.Inventory.Items[usedItem.ItemId];
        Player.SetHotbarSlot(ItemRConfig.PetSlotId, itemModel, ItemCatalog);
        SendXt("hs", Player.Character.Data.Hotbar);
    }

    private void HandleBomb(ItemDescription usedItem, Vector3Model position, int direction)
    {
        Player.CheckAchievement(AchConditionType.Bomb, string.Empty, InternalAchievement, Logger);
        Player.CheckAchievement(AchConditionType.Bomb, usedItem.PrefabName, InternalAchievement, Logger);

        Player.HandleDrop(ItemRConfig, TimerThread, Logger, usedItem, position, direction);

        var removeFromHotbar = true;

        if (usedItem.InventoryCategoryID is
            ItemFilterCategory.WeaponAndAbilities or
            ItemFilterCategory.Pets)
            removeFromHotbar = false;

        if (removeFromHotbar)
            RemoveFromHotbar(Player.Character, usedItem);
    }

    private void HandleConsumable(ItemDescription usedItem)
    {
        if (usedItem.ItemActionType == ItemActionType.Eat)
        {
            Player.CheckAchievement(AchConditionType.Consumable, string.Empty, InternalAchievement, Logger);
            Player.CheckAchievement(AchConditionType.Consumable, usedItem.PrefabName, InternalAchievement, Logger);
        }
        else if (usedItem.ItemActionType == ItemActionType.Drink)
        {
            Player.CheckAchievement(AchConditionType.Drink, string.Empty, InternalAchievement, Logger);
            Player.CheckAchievement(AchConditionType.Drink, usedItem.PrefabName, InternalAchievement, Logger);
        }

        Player.HandleItemEffect(usedItem, TimerThread, ItemRConfig, Logger);

        var removeFromHotbar = true;

        if (usedItem.InventoryCategoryID is
            ItemFilterCategory.WeaponAndAbilities or
            ItemFilterCategory.Pets)
            removeFromHotbar = false;

        if (removeFromHotbar)
            RemoveFromHotbar(Player.Character, usedItem);
    }

    private void HandleRecipe(ItemDescription usedItem)
    {
        Player.RemoveItem(usedItem, 1, ItemCatalog);

        var recipe = RecipeCatalog.GetRecipeById(usedItem.RecipeParentItemID);

        Player.Character.Data.RecipeList.RecipeList.Add(recipe);

        Player.SendXt("cz", recipe);
    }

    private void HandleSuperPack(ItemDescription usedItem)
    {
        foreach (var pair in VendorCatalog.GetSuperPacksItemQuantityMap(usedItem.ItemId))
        {
            var packItem = ItemCatalog.GetItemFromId(pair.Key);

            if (packItem == null)
                continue;

            Player.AddItem(packItem, pair.Value, ItemCatalog);
        }

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

        Player.SendUpdatedInventory();
    }
}
