using Server.Reawakened.Levels.Models.Entities;

namespace Server.Reawakened.Entities;

public class SpawnPointEntity : SynchronizedEntity<SpawnPoint>
{
    public int Index => EntityData.Index;
}
