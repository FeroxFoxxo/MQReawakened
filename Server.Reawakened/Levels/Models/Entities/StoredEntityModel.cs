using Server.Reawakened.Levels.Models.Planes;

namespace Server.Reawakened.Levels.Models.Entities;

public class StoredEntityModel
{
    public readonly Level Level;
    public readonly Microsoft.Extensions.Logging.ILogger Logger;

    public readonly int Id;
    public readonly string PrefabName;
    public readonly Vector3Model Position;

    public StoredEntityModel(int id, string prefabName, Vector3Model position, Level level, Microsoft.Extensions.Logging.ILogger logger)
    {
        Level = level;
        Logger = logger;
        Position = position;
        Id = id;
        PrefabName = prefabName;
    }
}
