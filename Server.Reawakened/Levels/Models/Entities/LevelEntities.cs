using Server.Reawakened.Levels.Models.LevelData;

namespace Server.Reawakened.Levels.Models.Entities;

public class LevelEntities
{
    public readonly Dictionary<int, ObjectInfoModel> SpawnPoints;

    public LevelEntities(LevelDataModel levelData)
    {
        if (levelData.Planes == null)
            return;
        
        SpawnPoints = levelData.Planes.SelectMany(p =>
                p.Value.GameObjects.Values.Where(g => g.ObjectInfo.Components.ContainsKey("SpawnPoint"))
            )
            .ToDictionary(
                g => int.Parse(g.ObjectInfo.Components["SpawnPoint"].ComponentAttributes["Index"]),
                g => g.ObjectInfo
            );
    }
}
