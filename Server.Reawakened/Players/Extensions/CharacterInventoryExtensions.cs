using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Players.Extensions;

public static class CharacterInventoryExtensions
{
    public static void HandleItemEffect(this Player player, ItemDescription usedItem, TimerThread timerThread, ItemRConfig config, ILogger<PlayerStatus> logger)
    {
        var effect = usedItem.ItemEffects.FirstOrDefault();
        if (usedItem.ItemEffects.Count > 0)
            player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
                                (int)effect.Type, effect.Value, effect.Duration, true, usedItem.PrefabName, true));

        switch (effect.Type)
        {
            case ItemEffectType.Healing:
            case ItemEffectType.HealthBoost:
            case ItemEffectType.IncreaseHealing:
            case ItemEffectType.Regeneration:
                if (player.Character.Data.CurrentLife >= player.Character.Data.MaxLife)
                    return;

                player.HealCharacter(usedItem, timerThread, config, effect.Type);
                break;
            case ItemEffectType.IncreaseAirDamage:
            case ItemEffectType.IncreaseAllResist:
            case ItemEffectType.Shield:
            case ItemEffectType.WaterBreathing:
            case ItemEffectType.PetRegainEnergy:
            case ItemEffectType.PetEnergyValue:
            case ItemEffectType.BananaMultiplier:
                player.TempData.BananaBoostsElixir = true;
                timerThread.DelayCall(SetBananaElixirTimer, player, TimeSpan.FromMinutes(30), TimeSpan.Zero, 1);
                break;
            case ItemEffectType.ExperienceMultiplier:
                player.TempData.ReputationBoostsElixir = true;
                timerThread.DelayCall(SetXpElixirTimer, player, TimeSpan.FromMinutes(30), TimeSpan.Zero, 1);
                break;
            case ItemEffectType.Defence:
                break;
            case ItemEffectType.Invisibility:
                player.TempData.Invisible = true;
                player.TemporaryInvisibility(usedItem.ItemEffects[(int)ItemFilterCategory.Consumables].Duration, timerThread);
                break;
            case ItemEffectType.Invalid:
            case ItemEffectType.Unknown:
            case ItemEffectType.Unknown_61:
            case ItemEffectType.Unknown_70:
            case ItemEffectType.Unknown_74:
            default:
                logger.LogError("Unknown ItemEffectType of ({effectType}) for item {usedItemName}", effect.Type, usedItem.PrefabName);
                return;
        }
        logger.LogInformation("Applied ItemEffectType of ({effectType}) from item {usedItemName} for _player {playerName}", effect.Type, usedItem.PrefabName, player.CharacterName);
    }

    public static void SetBananaElixirTimer(object playerObj)
    {
        var player = (Player)playerObj;

        if (player == null)
            return;

        if (player.TempData == null)
            return;

        player.TempData.BananaBoostsElixir = false;
    }

    public static void SetXpElixirTimer(object playerObj)
    {
        var player = (Player)playerObj;

        if (player == null)
            return;

        if (player.TempData == null)
            return;

        player.TempData.ReputationBoostsElixir = false;
    }

    public static bool TryGetItem(this CharacterModel characterData, int itemId, out ItemModel outItem) =>
        characterData.Data.Inventory.Items.TryGetValue(itemId, out outItem);

    public static void RemoveItem(this Player player, ItemDescription item, int count, ItemCatalog itemCatalog)
    {
        var characterData = player.Character;

        if (!characterData.TryGetItem(item.ItemId, out var gottenItem))
            return;

        gottenItem.Count -= count;

        player.CheckObjective(ObjectiveEnum.Inventorycheck, gottenItem.ItemId.ToString(), item.PrefabName, gottenItem.Count, itemCatalog);
    }
    
    public static void AddItem(this Player player, ItemDescription item, int count, ItemCatalog itemCatalog)
    {
        if (!itemCatalog.CanAddItem(item))
            return;

        var characterData = player.Character;

        if (!characterData.Data.Inventory.Items.ContainsKey(item.ItemId))
            characterData.Data.Inventory.Items.Add(item.ItemId, new ItemModel
            {
                ItemId = item.ItemId,
                Count = 0,
                BindingCount = item.BindingCount,
                DelayUseExpiry = item.DelayUseExpiry
            });

        if (!characterData.TryGetItem(item.ItemId, out var gottenItem))
            return;

        gottenItem.Count += count;

        player.CheckObjective(ObjectiveEnum.Inventorycheck, gottenItem.ItemId.ToString(), item.PrefabName, gottenItem.Count, itemCatalog);
    }

    public static void AddKit(this CharacterModel characterData, List<ItemDescription> items, int count)
    {
        foreach (var item in items)
        {
            if (item != null)
                if (characterData.Data.Inventory.Items.TryGetValue(item.ItemId, out var gottenKit))
                    gottenKit.Count += count;
                else
                    characterData.Data.Inventory.Items.Add(item.ItemId, new ItemModel
                    {
                        ItemId = item.ItemId,
                        Count = count,
                        BindingCount = item.BindingCount,
                        DelayUseExpiry = item.DelayUseExpiry
                    });
        }
    }

    public static string GetItemListString(this InventoryModel inventory)
    {
        var sb = new SeparatedStringBuilder('|');

        foreach (var item in inventory.Items)
            sb.Append(item.Value.ToString());

        return sb.ToString();
    }

    public static void SendUpdatedInventory(this Player player, bool fromEquippedUpdate = false)
    {
        player.SendXt(
            "ip",
            player.Character.Data.Inventory.GetItemListString(),
            fromEquippedUpdate
        );

        foreach (var item in player.Character.Data.Inventory.Items)
            if (item.Value.Count <= 0)
                player.Character.Data.Inventory.Items.Remove(item.Key);
    }

    public static void SetHotbarSlot(this Player player, int slotId, ItemModel itemModel, ItemCatalog catalog)
    {
        var hotbar = player.Character.Data.Hotbar;

        if (!hotbar.HotbarButtons.TryAdd(slotId, itemModel))
            hotbar.HotbarButtons[slotId] = itemModel;

        CheckPetEquip(player, itemModel.ItemId, true, catalog);
    }

    public static void RemoveHotbarSlot(this Player player, int slotId, ItemCatalog catalog)
    {
        var hotbar = player.Character.Data.Hotbar;

        if (!hotbar.HotbarButtons.TryGetValue(slotId, out var itemModel))
            return;

        hotbar.HotbarButtons.Remove(slotId);

        CheckPetEquip(player, itemModel.ItemId, false, catalog);
    }

    private static void CheckPetEquip(Player player, int itemId, bool equiped, ItemCatalog catalog)
    {
        var item = catalog.GetItemFromId(itemId);

        if (item.CategoryId != ItemCategory.Pet)
            return;

        foreach (var roomPlayer in player.Room.Players)
            roomPlayer.Value.SendXt("ZE", player.UserId, item.ItemId, equiped ? 1 : 0);

        player.Character.Data.PetItemId = equiped ? item.ItemId : 0;
    }
}
