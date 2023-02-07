using Server.Reawakened.Levels.Models.Planes;

namespace Server.Reawakened.Levels.Models.Entities;

public class StoredEntityModel
{
    public readonly Level Level;
    public readonly GameObjectModel GameObject;
    public readonly Microsoft.Extensions.Logging.ILogger Logger;

    public StoredEntityModel(GameObjectModel gameObject, Level level, Microsoft.Extensions.Logging.ILogger logger)
    {
        GameObject = gameObject;
        Level = level;
        Logger = logger;
    }
}
