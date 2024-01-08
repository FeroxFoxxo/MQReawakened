using A2m.Server;
using Server.Reawakened.Rooms.Extensions;

namespace Server.Reawakened.Players.Extensions;

public static class PlayerHealExtensions
{
    public static void HealCharacter(this Player player, ItemDescription usedItem)
    {
        foreach (var effect in usedItem.ItemEffects)
        {
            switch (effect.TypeId)
            {
                case 5:
                    HealOnce(player, usedItem);
                    break;
            }
            return;

            //Check if effect type is heal overtime.
            //Create HealOvertime method.
        }
    }

    public static void HealOnce(this Player player, ItemDescription usedItem)
    {
        Health_SyncEvent health = null;
        foreach (var effect in usedItem.ItemEffects)
        {
            var healValue = effect.Value;

            if (usedItem.ItemId == 396) //If healing staff, convert heal value.
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
