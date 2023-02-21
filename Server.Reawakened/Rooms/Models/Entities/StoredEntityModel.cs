using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Rooms.Models.Entities;

public class StoredEntityModel
{
    public readonly GameObjectModel GameObject;
    public readonly Room Room;

    public StoredEntityModel(GameObjectModel gameObject, Room room)
    {
        GameObject = gameObject;
        Room = room;
    }
}
