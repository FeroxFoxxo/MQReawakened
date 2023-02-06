using Server.Reawakened.Levels.Models;
using Server.Reawakened.Levels.Models.LevelData;

namespace Server.Reawakened.Levels.SyncedData.Abstractions;

public class StoredEntityModel
{
    public readonly Level Level;
    public readonly Microsoft.Extensions.Logging.ILogger Logger;
    
    public readonly int Id;
    public readonly Vector3Model Position;

    public StoredEntityModel(int id, Vector3Model position, Level level, Microsoft.Extensions.Logging.ILogger logger)
    {
        Level = level;
        Logger = logger;
        Position = position;
        Id = id;
    }
}
