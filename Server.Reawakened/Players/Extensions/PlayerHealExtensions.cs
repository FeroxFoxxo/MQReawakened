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
                HealOvertimeType(player, usedItem);
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

    private static async Task HealOvertimeType(Player player, ItemDescription usedItem)
    {
        var effect = usedItem.ItemEffects.FirstOrDefault();

        var potionDuration = effect.Duration;
        var healValue = effect.Value;
        switch (usedItem.SubCategoryId)
        {
            case ItemSubCategory.Usable:
                await Task.Delay(1000);

                HealOvertime(player, healValue, potionDuration);
                break;
            case ItemSubCategory.Potion:
                healValue = usedItem.ItemEffects[1].Value;

                HealOnce(player, usedItem);

                HealOvertime(player, healValue, potionDuration);
                break;
            case ItemSubCategory.Defensive:
                HealOnce(player, usedItem);
                break;
        }
    }
    public static async void HealOvertime(Player player, int itemEffectHealValue, int potionDuration)
    {
        var healthAddedUpOvertime = 0;
        var healthUntilMaxed = player.Character.Data.MaxLife - player.Character.Data.CurrentLife;

        while (healthAddedUpOvertime < itemEffectHealValue)
        {
            await Task.Delay(3000);

            var healthAddedOvertime = itemEffectHealValue / potionDuration * 3.4;

            healthAddedUpOvertime += (int)healthAddedOvertime;

            var healthLeftToHeal = itemEffectHealValue - healthAddedUpOvertime;

            if (healthAddedUpOvertime > itemEffectHealValue)
                healthAddedOvertime = healthLeftToHeal;

            if (healthUntilMaxed < healthAddedOvertime)
                healthAddedOvertime = healthUntilMaxed;

            player.Room.SendSyncEvent(new Health_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
                player.Character.Data.CurrentLife += Convert.ToInt32(healthAddedOvertime), player.Character.Data.MaxLife, "now"));
        }
    }
}
