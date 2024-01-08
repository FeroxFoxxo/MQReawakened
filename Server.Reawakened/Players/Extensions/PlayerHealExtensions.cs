using A2m.Server;
using Server.Reawakened.Configs;
using Server.Reawakened.Rooms.Extensions;

namespace Server.Reawakened.Players.Extensions;

public static class PlayerHealExtensions
{
    public static void HealCharacter(this Player player, ItemDescription usedItem, ServerRConfig serverConfig)
    {
        foreach (var effect in usedItem.ItemEffects)
        {
            switch ((ItemEffectType) effect.TypeId)
            {
                case ItemEffectType.Healing:
                    HealOnce(player, usedItem, serverConfig);
                    break;
            }
            return;

            //Check if effect type is heal overtime.
            //Create HealOvertime method.
        }
    }

    public static void HealOnce(this Player player, ItemDescription usedItem, ServerRConfig serverConfig)
    {
        Health_SyncEvent health = null;

        foreach (var effect in usedItem.ItemEffects)
        {
            var healValue = effect.Value;

            if (usedItem.ItemId == serverConfig.HealingStaff) //If healing staff, convert heal value.
                healValue = Convert.ToInt32(player.Character.Data.MaxLife / 3.527);

            var hpUntilMaxHp = player.Character.Data.MaxLife - player.Character.Data.CurrentLife;

            if (hpUntilMaxHp < healValue)
                healValue = hpUntilMaxHp;

            health = new Health_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
                    player.Character.Data.CurrentLife += healValue, player.Character.Data.MaxLife, "Now");
        }

        player.Room.SendSyncEvent(health);
    }
}
