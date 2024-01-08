using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;

namespace Server.Reawakened.Players.Extensions;

public static class PlayerDamageExtensions
{
    public static void ApplyCharacterDamage(this Player player, Room room, int damage)
    {
        player.Character.Data.CurrentLife -= damage;

        if (player.Character.Data.CurrentLife < 0)
        {
            player.Character.Data.CurrentLife = 0;
        }

        var health = new Health_SyncEvent(player.GameObjectId.ToString(), room.Time, player.Character.Data.CurrentLife, player.Character.Data.MaxLife, "Hurt");
        room.SendSyncEvent(health);
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
