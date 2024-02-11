using Server.Reawakened.Rooms;

namespace Server.Reawakened.Entities.Interfaces;

public interface IDestructible
{
    public void Destroy(Room room, string id);
}
