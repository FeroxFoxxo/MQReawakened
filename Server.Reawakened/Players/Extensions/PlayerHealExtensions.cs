using A2m.Server;
using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Timers;

namespace Server.Reawakened.Players.Extensions;

public static class PlayerHealExtensions
{
    public static void HealCharacter(this Player player, ItemDescription usedItem, TimerThread timerThread, ItemRConfig config, ItemEffectType effectType)
    {
        switch (effectType)
        {
            case ItemEffectType.Healing:
                HealOnce(player, usedItem, config);
                break;
            case ItemEffectType.Regeneration:
                HealOverTimeType(player, usedItem, timerThread, config);
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

    public static void HealOnce(Player player, ItemDescription usedItem, ItemRConfig config)
    {
        if (player == null || player.Room == null ||
            player.Character.CurrentLife >= player.Character.MaxLife)
            return;

        //Rejuvenation Potion's initial heal value is stored as the second element in the ItemEffects list.
        var healValue = usedItem.ItemEffects.Count == 1 ?
            usedItem.ItemEffects.FirstOrDefault().Value :
            usedItem.ItemEffects.LastOrDefault().Value;

        //If healing staff, convert heal value.
        if (usedItem.InventoryCategoryID == ItemFilterCategory.WeaponAndAbilities)
            healValue = GetHealValue(player, Convert.ToInt32(player.Character.MaxLife / config.HealingStaffHealValue));

        if (player.Character.CurrentLife + healValue >= player.Character.MaxLife)
            healValue = player.Character.MaxLife - player.Character.CurrentLife;

        player.Room.SendSyncEvent(new Health_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
                player.Character.Write.CurrentLife += healValue, player.Character.MaxLife, string.Empty));
    }

    public static void HealOverTime(Player player, ItemDescription usedItem, TimerThread timerThread)
    {
        if (player.Character.CurrentLife >= player.Character.MaxLife)
            return;

        var effect = usedItem.ItemEffects.FirstOrDefault();

        if (effect != null)
        {
            var healItemData = new ItemHealOverTimeData() { OverTimeHealValue = effect.Value, TotalTicks = effect.Duration / 3, Player = player };

            timerThread.RunInterval(OverTimeHealTicks, healItemData, TimeSpan.FromSeconds(3), healItemData.TotalTicks, TimeSpan.FromSeconds(3));
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

    private static void HealOverTimeType(Player player, ItemDescription usedItem, TimerThread timerThread, ItemRConfig config)
    {
        switch (usedItem.SubCategoryId)
        {
            case ItemSubCategory.Usable:
                HealOverTime(player, usedItem, timerThread);
                break;
            case ItemSubCategory.Potion:
                if (usedItem.ItemEffects.Count > 1)
                    HealOnce(player, usedItem, config);

                HealOverTime(player, usedItem, timerThread);
                break;
            case ItemSubCategory.Defensive:
                HealOnce(player, usedItem, config);
                break;
        }
    }

    private static int GetHealValue(Player player, int healValue)
    {
        var hpUntilMaxHp = player.Character.MaxLife - player.Character.CurrentLife;

        if (hpUntilMaxHp < healValue)
            healValue = hpUntilMaxHp;

        return healValue;
    }
}
