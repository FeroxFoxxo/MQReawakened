using A2m.Server;
using Server.Reawakened.Configs;
using Server.Reawakened.Rooms.Extensions;

namespace Server.Reawakened.Players.Extensions;

public static class PlayerHealExtensions
{
    public static void HealCharacter(this Player player, ItemDescription usedItem, ItemEffectType effectType)
    {
        switch (effectType)
        {
            case ItemEffectType.Healing:
                HealOnce(player, usedItem);
                break;
            case ItemEffectType.Regeneration:
                HealOvertime(player, usedItem);
                break;
        }
    }

    public static void HealOnce(Player player, ItemDescription usedItem)
    {
        var healValue = 0;

        if (usedItem.InventoryCategoryID == ItemFilterCategory.WeaponAndAbilities) //If healing staff, convert heal value.
            healValue = Convert.ToInt32(player.Character.Data.MaxLife / 3.527);

        var hpUntilMaxHp = player.Character.Data.MaxLife - player.Character.Data.CurrentLife;

        if (hpUntilMaxHp < healValue)
            healValue = hpUntilMaxHp;

        player.Room.SendSyncEvent(new Health_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
                player.Character.Data.CurrentLife += healValue, player.Character.Data.MaxLife, "Now"));
    }

    private static async Task HealOvertime(Player player, ItemDescription usedItem)
    {
        var effect = usedItem.ItemEffects.FirstOrDefault();

        var healthUntilMaxed = player.Character.Data.MaxLife - player.Character.Data.CurrentLife;
        var finalHpValue = 0;
        var healthAddedOvertime = 0;
        var potionDuration = effect.Duration;
        var overtimeHpValue = effect.Value;
        bool stopHealing;
        switch (usedItem.SubCategoryId)
        {
            case ItemSubCategory.Usable:
                await Task.Delay(1000);

                stopHealing = false;
                while (!stopHealing)
                {
                    await Task.Delay(3000);

                    var healthAddedOvertimeValue = overtimeHpValue / potionDuration * 3.4;

                    healthAddedOvertime += (int)healthAddedOvertimeValue;

                    var healthLeftToHeal = overtimeHpValue - healthAddedOvertime;

                    if (healthAddedOvertime > overtimeHpValue)
                        healthAddedOvertimeValue = finalHpValue;

                    finalHpValue = healthLeftToHeal;

                    if (healthUntilMaxed < healthAddedOvertimeValue)
                        healthAddedOvertimeValue = healthUntilMaxed;

                    player.Room.SendSyncEvent(new Health_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
                        player.Character.Data.CurrentLife += Convert.ToInt32(healthAddedOvertimeValue), player.Character.Data.MaxLife, "now"));

                    if (healthAddedOvertime >= overtimeHpValue)
                        stopHealing = true;
                }
                break;
            case ItemSubCategory.Potion:
                overtimeHpValue = usedItem.ItemEffects[1].Value;

                HealOnce(player, usedItem);

                stopHealing = false;
                healthAddedOvertime = 0;

                while (!stopHealing)
                {
                    await Task.Delay(3000);

                    var healthPerSecond = overtimeHpValue / potionDuration * 3.4;

                    healthAddedOvertime += (int)healthPerSecond;

                    var healthLeftToHeal = overtimeHpValue - healthAddedOvertime;

                    if (healthAddedOvertime > overtimeHpValue)
                        healthPerSecond = finalHpValue;

                    finalHpValue = healthLeftToHeal;

                    if (healthUntilMaxed < healthPerSecond)
                        healthPerSecond = healthUntilMaxed;

                    player.Room.SendSyncEvent(new Health_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
                        player.Character.Data.CurrentLife += Convert.ToInt32(healthPerSecond), player.Character.Data.MaxLife, "now"));

                    if (healthAddedOvertime >= overtimeHpValue)
                        stopHealing = true;
                }
                break;
            case ItemSubCategory.Defensive:
                HealOnce(player, usedItem);
                break;
        }
    }
}
