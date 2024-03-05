using Server.Reawakened.Players;
using Server.Reawakened.Rooms;

namespace Server.Reawakened.Entities.Interfaces;

public interface IDestructible
{
    public bool IsBroken { get; set; }
    public void Destroy(Player player, Room room, string id);
}
