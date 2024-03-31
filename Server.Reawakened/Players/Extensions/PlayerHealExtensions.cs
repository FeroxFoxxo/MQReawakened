using A2m.Server;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
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

    public static void HealOnce(Player player, ItemDescription usedItem, ItemRConfig config)
    {
        if (player == null || player.Room == null ||
            player.Character.Data.CurrentLife >= player.Character.Data.MaxLife)
            return;

        //Rejuvenation Potion's initial heal value is stored as the second element in the ItemEffects list.
        var healValue = usedItem.ItemEffects.Count == 1 ?
            usedItem.ItemEffects.FirstOrDefault().Value :
            usedItem.ItemEffects.LastOrDefault().Value;

        healValue = GetBuffedHealValue(player, healValue);

        //If healing staff, convert heal value.
        if (usedItem.InventoryCategoryID == ItemFilterCategory.WeaponAndAbilities)
            healValue = CheckMaxHealth(player, Convert.ToInt32(player.Character.Data.MaxLife / config.HealingStaffHealValue));

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
        public Player Player { get; } = player;
        public int OverTimeHealValue { get; } = overTimeHealValue;
        public int TotalTicks { get; } = totalTicks;
    }

    private static void OverTimeHealTicks(object itemData)
    {
        var itemHealData = (ItemHealOverTimeData)itemData;

        var player = itemHealData.Player;
        var tickHealValue = itemHealData.OverTimeHealValue / itemHealData.TotalTicks;

        if (player == null || player.Room == null ||
            player.Character.Data.CurrentLife >= player.Character.Data.MaxLife ||
            player.Character.Data.CurrentLife <= 0)
            return;

        tickHealValue = GetBuffedHealValue(player, tickHealValue);

        var healEvent = new Health_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
           player.Character.Data.CurrentLife += tickHealValue, player.Character.Data.MaxLife, string.Empty);

        player.Room.SendSyncEvent(healEvent);
    }
    //Original healing value >2011.
    private static int GetBuffedHealValue(Player player, int outOfDateHealValue)
    {
        var healValue = outOfDateHealValue * 18;

        return CheckMaxHealth(player, healValue);
    }
    //(This is really needed in the games current state or else healing items aren't worth buying or using)
    //Worth noting; Item descriptions in-game display 2011 stats because items load from an out of date ItemCatalogDict, instead of MiscTextDisc.

    private static int CheckMaxHealth(Player player, int healValue)
    {
        var hpUntilMaxHp = player.Character.Data.MaxLife - player.Character.Data.CurrentLife;

        if (hpUntilMaxHp < healValue)
            healValue = hpUntilMaxHp;

        return healValue;
    }
}
