using Server.Base.Logging;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Rooms.Models.Entities;

public class StoredEntityModel
{
    public readonly GameObjectModel GameObject;
    public readonly Room Room;
    public readonly FileLogger Logger;

    public StoredEntityModel(GameObjectModel gameObject, Room room, FileLogger logger)
    {
        GameObject = gameObject;
        Room = room;
        Logger = logger;
    }
}
