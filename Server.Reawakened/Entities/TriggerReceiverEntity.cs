using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Base.Network;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Enums;
using Server.Reawakened.Rooms.Models.Entities;
using System.Text;
using System;

namespace Server.Reawakened.Entities;

public class TriggerReceiverEntity : SyncedEntity<TriggerReceiver>, ITriggerable
{
    private int _activations;
    private int _deactivations;

    public bool Activated = true;
    public bool Enabled = true;
    public int NbActivationsNeeded => EntityData.NbActivationsNeeded;
    public int NbDeactivationsNeeded => EntityData.NbDeactivationsNeeded;
    public bool DisabledUntilTriggered => EntityData.DisabledUntilTriggered;
    public float DelayBeforeTrigger => EntityData.DelayBeforeTrigger;
    public bool ActiveByDefault => EntityData.ActiveByDefault;
    public TriggerReceiver.ReceiverCollisionType CollisionType => EntityData.CollisionType;

    public ILogger<TriggerReceiverEntity> Logger { get; set; }
    public FileLogger FileLogger { get; set; }

    public void TriggerStateChange(TriggerType triggerType, Room room, bool triggered)
    {
        Enabled = triggerType switch
        {
            TriggerType.Enable => true,
            TriggerType.Disable => false,
            _ => Enabled
        };
        var sb = new StringBuilder();

        sb.AppendLine($"Entity: {Id}")
            .AppendLine($"Enabled: {Enabled}")
            .AppendLine($"Activations: {_activations}/{NbActivationsNeeded}")
            .AppendLine($"Deactivations: {_deactivations}/{NbDeactivationsNeeded}")
            .Append($"Trigger: {triggerType}");

        FileLogger.WriteGenericLog<TriggerReceiver>("triggered-receivers", "Receiver Triggered", sb.ToString(), LoggerType.Trace);

        if (!Enabled)
            return;

        switch (triggerType)
        {
            case TriggerType.Activate:
                if (triggered)
                    _activations++;
                else
                    _activations--;
                break;
            case TriggerType.Deactivate:
                if (triggered)
                    _deactivations++;
                else
                    _deactivations--;
                break;
        }

        if (_activations >= NbActivationsNeeded)
            Trigger(true);
        else if (_deactivations >= NbDeactivationsNeeded)
            Trigger(false);
    }

    public override void InitializeEntity() => Trigger(ActiveByDefault);

    public override object[] GetInitData(NetState netState) => new object[] { Activated ? 1 : 0 };

    public void Trigger(bool activated)
    {
        Activated = activated;

        Logger.LogTrace("Triggering entity '{Id}' to {Bool}.", Id, Activated);

        var entities = Room.Entities[Id];

        foreach (var entity in entities)
        {
            if (activated)
            {
                if (entity is IMoveable moveable)
                    moveable.GetMovement().Activate(Room.Time);
            }
            else
            {
                if (entity is IMoveable moveable)
                    moveable.GetMovement().Deactivate(Room.Time);
            }
        }

        SendTriggerState(activated);
    }

    public void SendTriggerState(bool activated) =>
        Room.SendSyncEvent(new TriggerReceiver_SyncEvent(Id.ToString(), Room.Time, "now", activated, 0));
}
