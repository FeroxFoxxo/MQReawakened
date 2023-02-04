using Server.Base.Core.Extensions;
using Server.Reawakened.Configs;
using Server.Reawakened.Levels.Extensions;
using Server.Reawakened.Levels.Models.LevelData;
using System.Text.Json;

namespace Server.Reawakened.Levels.Models.Entities;

public class LevelEntities
{
    public Dictionary<int, ObjectInfoModel> SpawnPoints { get; set; }
    public Dictionary<int, ObjectInfoModel> Portals { get; set; }

    public LevelEntities(Level level, ServerConfig config)
    {
        if (level.LevelData.Planes == null)
            return;

        Portals = level.LevelData.Planes.SelectMany(p =>
                p.Value.GameObjects.Values.Where(g => g.ObjectInfo.Components.ContainsKey("PortalController"))
            )
            .ToDictionary(
                g => g.ObjectInfo.ObjectId,
                g => g.ObjectInfo
            ).OrderDictionary();

        SpawnPoints = level.LevelData.Planes.SelectMany(p =>
            p.Value.GameObjects.Values.Where(g => g.ObjectInfo.Components.ContainsKey("SpawnPoint"))
        ).ToDictionary(
            g => int.Parse(g.ObjectInfo.Components["SpawnPoint"].ComponentAttributes["Index"]),
            g => g.ObjectInfo
        ).OrderDictionary();

        GetDirectory.OverwriteDirectory(config.LevelEntitySaveDirectory);
        var path = Path.Join(config.LevelEntitySaveDirectory, $"{level.LevelInfo.Name}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true }));
    }
}
