using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Rooms.Extensions;

namespace Server.Reawakened.Players.Extensions;

public static class CharacterInventoryExtensions
{
    public static void HandleItemEffectBuff(this Player player, ItemDescription usedItem, TimerThread timerThread, ServerRConfig serverRConfig, ILogger<PlayerStatus> logger)
    {
        var effect = usedItem.ItemEffects.FirstOrDefault();

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
                            (int)effect.Type, effect.Value, effect.Duration, true, usedItem.PrefabName, true));

        var effectCategory = usedItem.ItemEffects.FirstOrDefault().Type;

        switch (effectCategory)
        {
            case ItemEffectType.Healing:
            case ItemEffectType.HealthBoost:
            case ItemEffectType.IncreaseHealing:
            case ItemEffectType.Regeneration:
                player.HealCharacter(usedItem, timerThread, serverRConfig, effectCategory);
                break;
            case ItemEffectType.IncreaseAirDamage:
            case ItemEffectType.IncreaseAllResist:
            case ItemEffectType.Shield:
            case ItemEffectType.WaterBreathing:
            case ItemEffectType.PetRegainEnergy:
            case ItemEffectType.PetEnergyValue:
            case ItemEffectType.BananaMultiplier:
            case ItemEffectType.ExperienceMultiplier:
            case ItemEffectType.Defence:
                break;
            case ItemEffectType.Invalid:
            case ItemEffectType.Unknown:
            case ItemEffectType.Unknown_61:
            case ItemEffectType.Unknown_70:
            case ItemEffectType.Unknown_74:
            default:
                logger.LogError("Unknown ItemEffectType of ({effectType}) for item {usedItemName}", effectCategory, usedItem.PrefabName);
                break;
        }

        logger.LogError("Applied ItemEffectType of ({effectType}) from item {usedItemName} for player {playerName}", effectCategory, usedItem.PrefabName, player.CharacterName);
    }

    public static bool TryGetItem(this CharacterModel characterData, int itemId, out ItemModel outItem) =>
        characterData.Data.Inventory.Items.TryGetValue(itemId, out outItem);

    public static void RemoveItem(this Player player, ItemDescription item, int count)
    {
        var characterData = player.Character;

        if (!characterData.TryGetItem(item.ItemId, out var gottenItem))
            return;

        gottenItem.Count -= count;

        player.CheckObjective(ObjectiveEnum.Inventorycheck, gottenItem.ItemId, item.PrefabName, gottenItem.Count);
    }

    public static void AddItem(this Player player, ItemDescription item, int count)
    {
        var characterData = player.Character;

        if (!characterData.Data.Inventory.Items.ContainsKey(item.ItemId))
            characterData.Data.Inventory.Items.Add(item.ItemId, new ItemModel
            {
                ItemId = item.ItemId,
                Count = 0,
                BindingCount = item.BindingCount,
                DelayUseExpiry = DateTime.MinValue
            });

        if (!characterData.TryGetItem(item.ItemId, out var gottenItem))
            return;

        gottenItem.Count += count;

        player.CheckObjective(ObjectiveEnum.Inventorycheck, gottenItem.ItemId, item.PrefabName, gottenItem.Count);
    }

    public static void AddKit(this CharacterModel characterData, List<ItemDescription> items, int count)
    {
        foreach (var item in items)
        {
            if (characterData.Data.Inventory.Items.TryGetValue(item.ItemId, out var gottenKit))
                gottenKit.Count += count;

            else
                characterData.Data.Inventory.Items.Add(item.ItemId, new ItemModel
                {
                    ItemId = item.ItemId,
                    Count = count,
                    BindingCount = item.BindingCount,
                    DelayUseExpiry = DateTime.MinValue
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

    public static void SendUpdatedInventory(this Player player, bool fromEquippedUpdate)
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
}
