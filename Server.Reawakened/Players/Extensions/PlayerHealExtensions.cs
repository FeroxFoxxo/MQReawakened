using A2m.Server;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Rooms.Extensions;

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
            var healItemData = new ItemHealOverTimeData(player, effect.Value, effect.Duration / 3);

            timerThread.DelayCall(OverTimeHealTicks, healItemData, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3), healItemData.TotalTicks);
        }
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

    private class ItemHealOverTimeData(Player player, int overTimeHealValue, int totalTicks)
    {
        public Player Player => player;
        public int OverTimeHealValue => overTimeHealValue;
        public int TotalTicks => totalTicks;
    }

    private static void OverTimeHealTicks(object itemData)
    {
        var itemHealData = (ItemHealOverTimeData)itemData;

        var player = itemHealData.Player;
        var tickHealValue = itemHealData.OverTimeHealValue / itemHealData.TotalTicks;

        if (player == null || player.Room == null ||
            player.Character.CurrentLife >= player.Character.MaxLife ||
            player.Character.CurrentLife <= 0)
            return;

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
