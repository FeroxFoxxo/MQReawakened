using Server.Reawakened.Players;

namespace Server.Reawakened.Rooms.Models.Entities;

public interface IDestructible
{
    public void Destroy(Player player, Room room, string id);
}
