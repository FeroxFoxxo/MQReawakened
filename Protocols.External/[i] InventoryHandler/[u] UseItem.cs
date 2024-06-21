using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Achievements;
using UnityEngine;

namespace Protocols.External._i__InventoryHandler;

public class UseItem : ExternalProtocol
{
    public override string ProtocolName => "iu";

    public VendorCatalog VendorCatalog { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public InternalRecipe RecipeCatalog { get; set; }
    public ItemRConfig ItemRConfig { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public TimerThread TimerThread { get; set; }
    public InternalAchievement InternalAchievement { get; set; }
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
        var itemModel = Player.Character.Inventory.Items[usedItem.ItemId];
        Player.SetHotbarSlot(ServerRConfig.PetHotbarIndex, itemModel, ItemRConfig);
        SendXt("hs", Player.Character.Hotbar);
    }

    private void HandleBomb(ItemDescription usedItem, Vector3 position, int direction)
    {
        Player.CheckAchievement(AchConditionType.Bomb, [usedItem.PrefabName], InternalAchievement, Logger);

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
        switch (usedItem.ItemActionType)
        {
            case ItemActionType.Eat:
                Player.CheckAchievement(AchConditionType.Consumable, [usedItem.PrefabName], InternalAchievement, Logger);
                break;
            case ItemActionType.Drink:
                Player.CheckAchievement(AchConditionType.Drink, [usedItem.PrefabName], InternalAchievement, Logger);
                break;
        }

        Player.HandleItemEffect(usedItem, TimerThread, ItemRConfig, ServerRConfig, Logger);

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
        Player.RemoveItem(usedItem, 1, ItemCatalog, ItemRConfig);

        var recipe = RecipeCatalog.GetRecipeById(usedItem.RecipeParentItemID);

        Player.Character.RecipeList.RecipeList.Add(recipe);

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
        var itemModel = character.Inventory.Items[item.ItemId];

        if (itemModel == null)
            return;

        if (itemModel.Count > 0)
            itemModel.Count--;

        else if (itemModel.Count <= 0)
            character.Inventory.Items.Remove(item.ItemId);

        Player.SendUpdatedInventory();
    }
}
