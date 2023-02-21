using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Rooms.Models.Entities;

public class StoredEntityModel
{
    public readonly GameObjectModel GameObject;
    public readonly Room Room;
    public readonly Microsoft.Extensions.Logging.ILogger Logger;

    public StoredEntityModel(GameObjectModel gameObject, Room room, Microsoft.Extensions.Logging.ILogger logger)
    {
        GameObject = gameObject;
        Room = room;
        Logger = logger;
    }
}
