using Server.Reawakened.Levels.Models.Entities;

namespace Server.Reawakened.Entities;

public class TriggerReceiverEntity : SyncedEntity<TriggerReceiver>
{
    public int NbActivationsNeeded => EntityData.NbActivationsNeeded;
    public int NbDeactivationsNeeded => EntityData.NbDeactivationsNeeded;
    public bool DisabledUntilTriggered => EntityData.DisabledUntilTriggered;
    public float DelayBeforeTrigger => EntityData.DelayBeforeTrigger;
    public bool ActiveByDefault => EntityData.ActiveByDefault;
    public TriggerReceiver.ReceiverCollisionType CollisionType => EntityData.CollisionType;
}
