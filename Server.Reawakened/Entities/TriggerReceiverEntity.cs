using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using System;
using System.Text;

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

        LogTriggerReciever(triggerType, triggered);
    }

    public void LogTriggerReciever(TriggerType triggerType, bool triggered)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Entity: {Id}")
            .AppendLine($"Enabled: {Enabled}");

        if (NbActivationsNeeded > 0)
            sb.AppendLine($"Activations: {_activations}/{NbActivationsNeeded}");

        if (NbDeactivationsNeeded > 0)
            sb.AppendLine($"Deactivations: {_deactivations}/{NbDeactivationsNeeded}");

        if (DisabledUntilTriggered)
            sb.AppendLine($"Disable Until Trigger: {DisabledUntilTriggered}");

        if (DelayBeforeTrigger > 0)
            sb.AppendLine($"Delay Before Trigger: {DelayBeforeTrigger}");

        sb.AppendLine($"Collision Type: {Enum.GetName(CollisionType)}")
            .AppendLine($"Trigger: {triggerType}");

        FileLogger.WriteGenericLog<TriggerReceiver>(
            "triggered-receivers",
            $"[Receiver {(triggered ? "Activation" : "Deactivation")} Trigger]",
            sb.ToString(),
            LoggerType.Trace
        );
    }

    public override void InitializeEntity() => Trigger(ActiveByDefault);

    public override object[] GetInitData(Player player) => new object[] { Activated ? 1 : 0 };

    public void LogTriggerRecieved()
    {
        var sb2 = new StringBuilder();

        sb2.AppendLine($"Triggered: {Activated}");

        FileLogger.WriteGenericLog<TriggerReceiver>("reciever-triggered", $"[Reciever {Id}]", sb2.ToString(),
            LoggerType.Trace);
    }

    public void Trigger(bool activated)
    {
        Activated = activated;

        LogTriggerRecieved();

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
