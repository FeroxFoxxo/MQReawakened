using Server.Reawakened.Levels.SyncedData.Abstractions;

namespace Server.Reawakened.Levels.SyncedData.Entities;

public class PortalControllerEntity : SynchronizedEntity<PortalController>
{
    public PortalControllerEntity(StoredEntityModel storedEntity,
        PortalController entityData) : base(storedEntity, entityData) { }
}
