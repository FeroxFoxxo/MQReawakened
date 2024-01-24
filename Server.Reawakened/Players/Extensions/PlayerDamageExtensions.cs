using A2m.Server;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;

namespace Server.Reawakened.Players.Extensions;

public static class PlayerDamageExtensions
{
    public static void ApplyUnderwaterDamage(this Player player, object _)
    {
        if (player.TempData.UnderWater == false) return;

        var damage = Convert.ToInt32(player.Character.Data.MaxLife / 10);

        player.ApplyCharacterDamage(player.Room, damage);

        var WaterDmgStatusEffect = new StatusEffect_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
            (int)ItemEffectType.WaterDamage, damage, 1, true, string.Empty, false);
        player.SendSyncEventToPlayer(WaterDmgStatusEffect);
    }
    public static void ApplyCharacterDamage(this Player player, Room room, int damage)
    {
        if (player.Character.Data.CurrentLife < 0)
            player.Character.Data.CurrentLife = 0;

        var waterDamage = new Health_SyncEvent(player.GameObjectId.ToString(), room.Time,
            player.Character.Data.CurrentLife -= damage, player.Character.Data.MaxLife, "Hurt");
        player.SendSyncEventToPlayer(waterDamage);
    }
    public static void ApplyDamageByPercent(this Player player, Room room, double percentage)
    {
        var health = (double)player.Character.Data.MaxLife;

        var damage = Convert.ToInt32(Math.Round(health * percentage));

        ApplyCharacterDamage(player, room, damage);
    }

    public static void ApplyDamageByObject(this Player player, Room room, int objectId)
    {
        //temporary code until enemy/hazard system is implemented
        ApplyDamageByPercent(player, room, .10);
    }
}
