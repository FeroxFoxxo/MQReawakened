using Server.Reawakened.Levels.SyncedData.Abstractions;

namespace Server.Reawakened.Levels.SyncedData.Entities;

public class SpawnPointEntity : SynchronizedEntity<SpawnPoint>
{
    public int Index => EntityData.Index;

    public SpawnPointEntity(StoredEntityModel storedEntity,
        SpawnPoint entityData) : base(storedEntity, entityData) { }
}
