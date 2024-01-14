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

    public static TimerThread TimerThread { get; set; }
    public static void HealCharacter(this Player player, ItemDescription usedItem, ItemEffectType effectType)
    {
        switch (effectType)
        {
            case ItemEffectType.Healing:
                HealOnce(player, usedItem, false);
                break;
            case ItemEffectType.Regeneration:
                HealOvertimeType(player, usedItem);
                break;
        }
    }

    public static void HealOnce(Player player, ItemDescription usedItem, bool isRejuvenationPotion)
    {
        var healValue = isRejuvenationPotion ? usedItem.ItemEffects[1].Value : usedItem.ItemEffects.FirstOrDefault().Value;

        if (usedItem.InventoryCategoryID == ItemFilterCategory.WeaponAndAbilities) //If healing staff, convert heal value.
            healValue = Convert.ToInt32(player.Character.Data.MaxLife / 3.527);

        var hpUntilMaxHp = player.Character.Data.MaxLife - player.Character.Data.CurrentLife;

        if (hpUntilMaxHp < healValue)
            healValue = hpUntilMaxHp;

        player.Room.SendSyncEvent(new Health_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
                player.Character.Data.CurrentLife += healValue, player.Character.Data.MaxLife, "Now"));
    }

    private static async Task HealOvertimeType(Player player, ItemDescription usedItem)
    {
        switch (usedItem.SubCategoryId)
        {
            case ItemSubCategory.Usable:
                await Task.Delay(1000);

                HealOverTime(player, usedItem);
                break;
            case ItemSubCategory.Potion:

                HealOnce(player, usedItem, true);

                HealOverTime(player, usedItem);
                break;
            case ItemSubCategory.Defensive:
                HealOnce(player, usedItem, false);
                break;
        }
    }

    public static async void HealOverTime(Player player, ItemDescription usedItem)
    {
        var effect = usedItem.ItemEffects.FirstOrDefault();

        var overTimeHealValue = effect.Value;
        var potionDuration = effect.Duration;

        var totalTicks = potionDuration / 3;

        var healItemdata = new ItemHealOverTimeData()
        {
            Effect = effect,
            Player = player,
            PrefabName = usedItem.PrefabName,
            ObjectId = usedItem.ItemId,
            OvertimeHealValue = overTimeHealValue,
            Duration = potionDuration,
            TotalTicks = totalTicks
        };

        for (var healTick = healItemdata.TotalTicks; healTick > 0; healTick--)
        {
            await Task.Delay(3000); //Convert to TimerThread.
            OvertimeHealTicks(healItemdata);
            //TimerThread.DelayCall(OvertimeHealTicks, healItemdata, TimeSpan.FromMilliseconds(3000), TimeSpan.Zero, 1);
        }


    }

    private class ItemHealOverTimeData()
    {
        public ItemEffect Effect { get; set; }
        public Player Player { get; set; }
        public string PrefabName { get; set; }
        public int ObjectId { get; set; }
        public int OvertimeHealValue { get; set; }
        public int Duration { get; set; }
        public int TotalTicks { get; set; }
    }

    public static void OvertimeHealTicks(object itemData)
    {
        var itemHealData = (ItemHealOverTimeData)itemData;
        var player = itemHealData.Player;

        var overTimeHealValue = itemHealData.Effect.Value;
        var potionDuration = itemHealData.Effect.Duration;

        var totalTicks = potionDuration / 3;
        var tick = overTimeHealValue / totalTicks;

        player.Room.SendSyncEvent(new Health_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
            player.Character.Data.CurrentLife += tick, player.Character.Data.MaxLife, "now"));
    }

}
