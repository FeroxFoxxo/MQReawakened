using Server.Reawakened.Rooms;

namespace Server.Reawakened.Entities.Components.GameObjects.InterObjs.Interfaces;

public interface IDestructible
{
    public void Destroy(Room room, string id);
}
