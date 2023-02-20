using Microsoft.Extensions.Logging;
using Server.Reawakened.Levels;
using Server.Reawakened.Levels.Enums;
using Server.Reawakened.Levels.Models.Entities;

namespace Server.Reawakened.Entities;

public class TriggerReceiverEntity : SyncedEntity<TriggerReceiver>, ITriggerable
{
    public int NbActivationsNeeded => EntityData.NbActivationsNeeded;
    public int NbDeactivationsNeeded => EntityData.NbDeactivationsNeeded;
    public bool DisabledUntilTriggered => EntityData.DisabledUntilTriggered;
    public float DelayBeforeTrigger => EntityData.DelayBeforeTrigger;
    public bool ActiveByDefault => EntityData.ActiveByDefault;
    public TriggerReceiver.ReceiverCollisionType CollisionType => EntityData.CollisionType;

    public ILogger<TriggerReceiverEntity> Logger { get; set; }

    public override void InitializeEntity() => IsActive = ActiveByDefault;

    public void TriggerStateChange(TriggerType triggerType, Level level) =>
        Logger.LogError("Trigger not implemented for {TypeName} (tried to change to {TriggerType}).",
            GetType().Name, triggerType);
}
