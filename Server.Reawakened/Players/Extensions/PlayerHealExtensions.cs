using A2m.Server;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using System;

namespace Server.Reawakened.Players.Extensions;

public static class PlayerHealExtensions
{
    public static void HealCharacter(this Player player, ItemDescription usedItem, TimerThread timerThread, ServerRConfig serverRConfig, ItemEffectType effectType)
    {
        switch (effectType)
        {
            case ItemEffectType.Healing:
                HealOnce(player, usedItem, serverRConfig);
                break;
            case ItemEffectType.Regeneration:
                HealOverTimeType(player, usedItem, timerThread, serverRConfig);
                break;
        }
    }

    public static void HealOnce(Player player, ItemDescription usedItem, ServerRConfig serverRConfig)
    {
        if (player.Character.Data.CurrentLife >= player.Character.Data.MaxLife)
            return;

        var healValue = usedItem.ItemEffects.Count > 1 ? usedItem.ItemEffects.LastOrDefault().Value : usedItem.ItemEffects.FirstOrDefault().Value; ; //Rejuvenation Potion's initial heal value is stored as the second element in the ItemEffects list.

        if (usedItem.InventoryCategoryID == ItemFilterCategory.WeaponAndAbilities) //If healing staff, convert heal value.
            healValue = Convert.ToInt32(player.Character.Data.MaxLife / serverRConfig.HealingStaffHealValue);

        var hpUntilMaxHp = player.Character.Data.MaxLife - player.Character.Data.CurrentLife;

        if (hpUntilMaxHp < healValue)
            healValue = hpUntilMaxHp;

        player.Room.SendSyncEvent(new Health_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
                player.Character.Data.CurrentLife += healValue, player.Character.Data.MaxLife, string.Empty));
    }

    public static void HealOverTime(Player player, ItemDescription usedItem, TimerThread timerThread)
    {
        if (player.Character.Data.CurrentLife >= player.Character.Data.MaxLife)
            return;

        var effect = usedItem.ItemEffects.FirstOrDefault();
        if (effect != null)
        {
            var healItemData = new ItemHealOverTimeData(player, effect.Value, effect.Duration / 3);

            timerThread.DelayCall(OverTimeHealTicks, healItemData, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3), healItemData.TotalTicks);
        }
    }

    private static void HealOverTimeType(Player player, ItemDescription usedItem, TimerThread timerThread, ServerRConfig serverRConfig)
    {
        switch (usedItem.SubCategoryId)
        {
            case ItemSubCategory.Usable:
                HealOverTime(player, usedItem, timerThread);
                break;
            case ItemSubCategory.Potion:
                if (usedItem.ItemEffects.Count > 1)
                    HealOnce(player, usedItem, serverRConfig);

                HealOverTime(player, usedItem, timerThread);
                break;
            case ItemSubCategory.Defensive:
                HealOnce(player, usedItem, serverRConfig);
                break;
        }
    }

    private class ItemHealOverTimeData
    {
        public ItemHealOverTimeData(Player player, int overTimeHealValue, int totalTicks)
        {
            Player = player;
            OverTimeHealValue = overTimeHealValue;
            TotalTicks = totalTicks;
        }

        public Player Player { get; }
        public int OverTimeHealValue { get; }
        public int TotalTicks { get; }
    }

    private static void OverTimeHealTicks(object itemData)
    {
        if (itemData is ItemHealOverTimeData itemHealData)
        {
            var player = itemHealData.Player;
            var tickHealValue = itemHealData.OverTimeHealValue / itemHealData.TotalTicks;

            player.Room.SendSyncEvent(new Health_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
               player.Character.Data.CurrentLife += tickHealValue, player.Character.Data.MaxLife, string.Empty));
        }
    }
}
