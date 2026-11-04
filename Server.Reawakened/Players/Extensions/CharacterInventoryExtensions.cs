using A2m.Server;
using Microsoft.Extensions.Logging;
using PetDefines;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Models.Pets;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Server.Reawakened.Players.Extensions;

public static class CharacterInventoryExtensions
{
    public static void HandleItemEffect(this Player player, ItemDescription usedItem, TimerThread timerThread,
        ServerRConfig serverRConfig, ILogger<PlayerStatus> logger)
    {
        var sendFx = true;
        foreach (var itemEffect in usedItem.ItemEffects)
        {
            switch (itemEffect.Type)
            {
                case ItemEffectType.PetRegainEnergy:
                    // Pet energy/abilities doesn't exist yet
                    if (serverRConfig.GameVersion <= GameVersion.vMinigames2012)
                        break;
                    
                    if (!player.Character.Pets.TryGetValue(player.Character.PetItemId.ToString(), out var pet))
                    {
                        logger.LogWarning("Couldn't find equipped pet for {characterName}", player.CharacterName);
                        return;
                    }
                    
                    pet.EatSnack(player, itemEffect.Value);
                    break;
                case ItemEffectType.Healing:
                case ItemEffectType.HealthBoost:
                case ItemEffectType.IncreaseHealing:
                case ItemEffectType.Regeneration:
                    if (player.Character.CurrentLife >= player.Character.MaxLife)
                        return;

                    player.HealCharacter(usedItem, itemEffect, timerThread, itemEffect.Type);
                    break;
                case ItemEffectType.IncreaseBluntDamage:
                case ItemEffectType.IncreaseAirDamage:
                case ItemEffectType.IncreaseFireDamage:
                case ItemEffectType.IncreaseEarthDamage:
                case ItemEffectType.IncreaseIceDamage:
                case ItemEffectType.IncreaseLightningDamage:
                case ItemEffectType.IncreaseAllResist:
                case ItemEffectType.Defence:
                case ItemEffectType.ResistAir:
                case ItemEffectType.ResistFire:
                case ItemEffectType.ResistEarth:
                case ItemEffectType.ResistIce:
                case ItemEffectType.ResistLightning:
					player.Character.StatusEffects.Add(itemEffect, usedItem.PrefabName);
                    sendFx = false;
                    break;
                case ItemEffectType.WaterBreathing:
                case ItemEffectType.Detect:
                case ItemEffectType.Invisibility:
                case ItemEffectType.BananaMultiplier:
                case ItemEffectType.ExperienceMultiplier:
                    player.Character.StatusEffects.Add(itemEffect, usedItem.PrefabName);
                    break;
                case ItemEffectType.ColorTonic:
                    // ColorTonic effect doesn't exist yet
                    if (serverRConfig.GameVersion <= GameVersion.vLate2012)
                        break;
                    
                    if (player.Character.StatusEffects.HasEffect(ItemEffectType.ColorTonic))
                    {
                        sendFx = false;

                        player.Character.StatusEffects.Remove(ItemEffectType.ColorTonic);

                        player.SendItemEffectToPlayer(itemEffect, string.Empty, sendFx, true);
                    }

                    player.Character.StatusEffects.Add(itemEffect, usedItem.PrefabName);
                    sendFx = true;
                    break;
                case ItemEffectType.Invalid:
                case ItemEffectType.Unknown:
                case ItemEffectType.Unknown_61:
                case ItemEffectType.Unknown_70:
                case ItemEffectType.Unknown_74:
                default:
                    logger.LogError("Unknown ItemEffectType of ({effectType}) for item {usedItemName}", itemEffect.Type, usedItem.PrefabName);
                    return;
            }

            player.SendItemEffectToPlayer(itemEffect, usedItem.PrefabName, sendFx, usedItem.Currency == CurrencyType.NickCash);

            logger.LogInformation("Applied ItemEffectType of ({effectType}) from item {usedItemName} for _player {playerName}",
                itemEffect.Type, usedItem.PrefabName, player.CharacterName);
        }
    }

    public static bool TryGetItem(this CharacterModel characterData, int itemId, out ItemModel outItem) =>
        characterData.Inventory.Items.TryGetValue(itemId, out outItem);

    public static void RemoveItem(this Player player, ItemDescription item, int count, ItemCatalog itemCatalog, ItemRConfig config)
    {
        var characterData = player.Character;

        if (!characterData.TryGetItem(item.ItemId, out var gottenItem))
            return;

        gottenItem.Count -= count;

        player.CheckObjective(ObjectiveEnum.Inventorycheck, gottenItem.ItemId.ToString(), item.PrefabName, gottenItem.Count, itemCatalog);
    }

    public static void AddItem(this Player player, ItemDescription item, int count, ItemCatalog itemCatalog)
    {
        if (!itemCatalog.CanAddItem(item) || item == null || player == null || itemCatalog == null)
            return;

        var characterData = player.Character;
        var itemCount = count;

        if (!characterData.Inventory.Items.ContainsKey(item.ItemId))
            characterData.Inventory.Items.Add(item.ItemId, new ItemModel(item.ItemId, count, item.BindingCount, item.DelayUseExpiry));

        else if (characterData.TryGetItem(item.ItemId, out var gottenItem))
        {
            gottenItem.Count += count;
            itemCount = gottenItem.Count;
        }

        player.CheckObjective(ObjectiveEnum.Inventorycheck, item.ItemId.ToString(), item.PrefabName, itemCount, itemCatalog);
    }

    public static void AddKit(this CharacterModel characterData, List<ItemDescription> items, int count)
    {
        foreach (var item in items)
        {
            if (item != null)
                if (characterData.Inventory.Items.TryGetValue(item.ItemId, out var gottenKit))
                    gottenKit.Count += count;
                else
                    characterData.Inventory.Items.Add(item.ItemId, new ItemModel(item.ItemId, count, item.BindingCount, item.DelayUseExpiry));
        }
    }

    public static string GetItemListString(this InventoryModel inventory)
    {
        var sb = new SeparatedStringBuilder('|');

        foreach (var item in inventory.Items)
            if (item.Value is not null)
                sb.Append(item.Value.ToString());

        return sb.ToString();
    }

    public static void SendUpdatedInventory(this Player player, bool fromEquippedUpdate = false)
    {
        player.SendXt(
            "ip",
            player.Character.Inventory.GetItemListString(),
            fromEquippedUpdate ? 1 : 0
        );

        foreach (var item in player.Character.Inventory.Items)
            if (item.Value != null && item.Value.Count <= 0)
                player.Character.Inventory.Items.Remove(item.Key);
    }

    public static void UseItemFromHotBar(this Player player, int itemId, ItemCatalog itemCatalog, ItemRConfig config)
    {
        var itemDescription = itemCatalog.GetItemFromId(itemId);

        player.RemoveItem(itemDescription, 1, itemCatalog, config);
        player.SendXt("hu", player.Character.Hotbar);
        player.SendUpdatedInventory(true);
    }

    public static void SetHotbarSlot(this Player player, int slotId, ItemModel itemModel, bool swappedItem = false)
    {
        var hotbar = player.Character.Hotbar;

        // Remove slot if it is not from the swap slot protocol
        if (!swappedItem)
            foreach (var hotbarSlot in hotbar.HotbarButtons.Where
                         (slot => slot.Key != slotId && slot.Value.ItemId == itemModel.ItemId))
                player.SetEmptySlot(hotbarSlot.Key);

        hotbar.HotbarButtons.TryAdd(slotId, itemModel);
        hotbar.HotbarButtons[slotId] = itemModel;
    }

    public static void SetEmptySlot(this Player player, int slotId)
    {
        var hotbar = player.Character.Hotbar;

        if (!hotbar.HotbarButtons.ContainsKey(slotId))
            return;

        hotbar.HotbarButtons.Remove(slotId);
    }

    public static void EquipPet(this Player player, string petId, PetAbilityParams petAbilityParams, ItemCatalog itemCatalog,
        WorldStatistics worldStatistics, ServerRConfig serverRConfig, ItemRConfig itemRConfig)
    {
        if (player == null || !player.Character.Hotbar.HotbarButtons.ContainsKey(4))
            return;

        if (petId == "0" || petId == string.Empty || itemCatalog.GetItemFromId(int.Parse(petId)) == null ||
            !itemCatalog.GetItemFromId(int.Parse(petId)).IsPet()) return;

        var refillEnergy = false;

        if (!player.Character.Pets.TryGetValue(petId, out var currentPet))
        {
            player.Character.Pets.Add(petId, currentPet = new PetModel());
            refillEnergy = true;
        }

        player.Character.Write.PetItemId = int.Parse(petId);

        currentPet.SpawnPet(player, petAbilityParams, true, refillEnergy, itemCatalog,
            worldStatistics, itemRConfig);
    }

    public static void UnequipPet(this Player player, string petId, PetAbilityParams petAbilityParams,
        ItemCatalog itemCatalog, WorldStatistics worldStatistics, ItemRConfig itemRConfig)
    {
        if (player == null || itemCatalog == null || string.IsNullOrEmpty(petId) || petId == "0") return;

        if (!int.TryParse(petId, out var parsedPetId)) return;

        var petItem = itemCatalog.GetItemFromId(parsedPetId);
        if (petItem == null || !petItem.IsPet()) return;

        if (!player.Character.Pets.TryGetValue(petId, out var currentPet)) return;

        currentPet.SpawnPet(player, petAbilityParams, false, false, itemCatalog, worldStatistics, itemRConfig);

        if (!player.Character.Hotbar.HotbarButtons.ContainsKey(4))
            player.Character.Write.PetItemId = 0;
    }

    public static string GetItemIdOfEquippedPet(this Player player) => player?.Character == null ? "0" : player.Character.PetItemId.ToString();

    public static string GetGameObjectIdOfEquippedPet(this Player player) =>
        player != null && player.Character.Pets.TryGetValue(player.GetItemIdOfEquippedPet(), out var pet)
            ? pet.GameObjectId : "0";

    public static void SwapSlots(this Player player, int slotId, ItemModel item)
    {
        foreach (var hotbarKVP in player.Character.Hotbar.HotbarButtons)
        {
            if (!player.Character.Hotbar.HotbarButtons.ContainsKey(slotId))
            {
                player.SetHotbarSlot(slotId, item);
                break;
            }
            
            if (hotbarKVP.Value.ItemId == item.ItemId)
            {
                var otherItem = player.Character.Hotbar.HotbarButtons[slotId];

                player.SetHotbarSlot(hotbarKVP.Key, otherItem, true);
                player.SetHotbarSlot(slotId, item, true);
            }
        }
        
        player.SendXt("hw", player.Character.Hotbar);
    }
}
