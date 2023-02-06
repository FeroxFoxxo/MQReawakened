using Server.Reawakened.Levels.SyncedData.Abstractions;

namespace Server.Reawakened.Levels.SyncedData.Entities;

public class PortalControllerEntity : SynchronizedEntity<PortalController>
{
    public string OverrideCondition => EntityData.OverrideCondition;
    public bool LaunchMinigame => EntityData.LaunchMinigame;
    public string TimedEventId => EntityData.timedEventId;
    public string TimedEventPortalObjectName => EntityData.TimedEventPortalObjectName;
    public string TimedEventPortalOnAnim => EntityData.TimedEventPortalOnAnim;
    public string TimedEventPortalOffAnim => EntityData.TimedEventPortalOffAnim;

    public PortalControllerEntity(StoredEntityModel storedEntity,
        PortalController entityData) : base(storedEntity, entityData) { }
}
