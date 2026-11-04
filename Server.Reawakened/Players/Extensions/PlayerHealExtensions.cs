using A2m.Server;
using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Timers;

namespace Server.Reawakened.Players.Extensions;

public static class PlayerHealExtensions
{
    public static void HealCharacter(this Player player, ItemDescription usedItem, ItemEffect itemEffect,
        TimerThread timerThread, ItemEffectType effectType)
    {
        if (player.TempData.IsKnockedOut)
            return;

        switch (effectType)
        {
            case ItemEffectType.Regeneration:
                HealOverTimeType(player, usedItem, itemEffect, timerThread, 0);
                break;
            default:
                HealOnce(player, itemEffect.Value);
                break;
        }
    }

    public static void PetHeal(this Player player, int healValue)
    {
        if (player == null || player.Room == null ||
          player.Character.CurrentLife >= player.Character.MaxLife)
            return;

        player.Room.SendSyncEvent(new Health_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
                player.Character.Write.CurrentLife += GetHealValue(player, healValue), player.Character.MaxLife, string.Empty));
    }

    public static void HealOnce(Player player, int healValue)
    {
        if (player == null || player.Room == null ||
            player.Character.CurrentLife >= player.Character.MaxLife)
            return;

        if (healValue < 0)
            healValue = 0;

        if (player.Character.CurrentLife + healValue >= player.Character.MaxLife)
            healValue = player.Character.MaxLife - player.Character.CurrentLife;

         player.Room.SendSyncEvent(new Health_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
                player.Character.Write.CurrentLife += healValue, player.Character.MaxLife, player.GameObjectId));
    }

    public static void HealOverTime(Player player, ItemEffect itemEffect, TimerThread timerThread, int healEffectValue)
    {
        if (player.Character.CurrentLife >= player.Character.MaxLife)
            return;

        if (healEffectValue < 0)
            healEffectValue = 0;

        var healItemData = new ItemHealOverTimeData() { OverTimeHealValue = healEffectValue, TotalTicks = itemEffect.Duration, Player = player };

        timerThread.RunInterval(OverTimeHealTicks, healItemData, TimeSpan.FromSeconds(1), healItemData.TotalTicks, TimeSpan.FromSeconds(1));
    }

    private static void HealOverTimeType(Player player, ItemDescription usedItem, ItemEffect itemEffect, TimerThread timerThread, int healBonus)
    {
        switch (usedItem.SubCategoryId)
        {
            case ItemSubCategory.Usable:
            case ItemSubCategory.Offensive:
                HealOverTime(player, itemEffect, timerThread, itemEffect.Value);
                break;
            case ItemSubCategory.Potion:
                if (usedItem.ItemEffects.Count > 1)
                    HealOnce(player, itemEffect.Value);

                HealOverTime(player, itemEffect, timerThread, itemEffect.Value);
                break;
            case ItemSubCategory.Defensive:
                HealOnce(player, itemEffect.Value);
                break;
        }
    }

    private class ItemHealOverTimeData : PlayerRoomTimer
    {
        public int OverTimeHealValue { get; set; }
        public int TotalTicks { get; set; }
    }

    private static void OverTimeHealTicks(ITimerData data)
    {
        var heal = (ItemHealOverTimeData)data;

        if (heal == null)
            return;

        if (heal.Player == null)
            return;

        if (heal.Player.Character == null)
            return;

        if (heal.Player.Room == null)
            return;

        if (!heal.Player.Room.IsOpen)
            return;

        var player = heal.Player;
        var tickHealValue = heal.OverTimeHealValue / heal.TotalTicks;

        if (player.Character.CurrentLife >= player.Character.MaxLife ||
            player.Character.CurrentLife <= 0)
            return;

        if (player.Character.CurrentLife + tickHealValue >= player.Character.MaxLife)
            tickHealValue = player.Character.MaxLife - player.Character.CurrentLife;

        var healEvent = new Health_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
           player.Character.Write.CurrentLife += tickHealValue, player.Character.MaxLife, string.Empty);

        player.Room.SendSyncEvent(healEvent);
    }

    private static int GetHealValue(Player player, int healValue)
    {
        var hpUntilMaxHp = player.Character.MaxLife - player.Character.CurrentLife;

        if (hpUntilMaxHp < healValue)
            healValue = hpUntilMaxHp;

        return healValue;
    }
}
