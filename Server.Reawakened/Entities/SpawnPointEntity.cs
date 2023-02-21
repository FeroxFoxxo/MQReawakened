using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities;

public class SpawnPointEntity : SyncedEntity<SpawnPoint>
{
    public int Index => EntityData.Index;
}
