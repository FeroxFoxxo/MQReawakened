using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using System.Text;

namespace Server.Reawakened.Entities.Components;

public class TriggerReceiverComp : Component<TriggerReceiver>, ICoopTriggered
{
    private int _activations;
    private int _deactivations;

    public bool Activated = true;
    public bool Enabled = true;
    public int NbActivationsNeeded => ComponentData.NbActivationsNeeded;
    public int NbDeactivationsNeeded => ComponentData.NbDeactivationsNeeded;
    public bool DisabledUntilTriggered => ComponentData.DisabledUntilTriggered;
    public float DelayBeforeTrigger => ComponentData.DelayBeforeTrigger;
    public bool ActiveByDefault => ComponentData.ActiveByDefault;
    public TriggerReceiver.ReceiverCollisionType CollisionType => ComponentData.CollisionType;

    public ILogger<TriggerReceiverComp> Logger { get; set; }
    public FileLogger FileLogger { get; set; }

    public override void InitializeComponent()
    {
        base.InitializeComponent();
        Trigger(ActiveByDefault);
    }

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        if (syncEvent.Type != SyncEvent.EventType.TriggerReceiver)
            return;

        var tEvent = new TriggerReceiver_SyncEvent(syncEvent);

        Trigger(tEvent.Activate);
    }

    public void TriggerStateChange(TriggerType triggerType, bool triggered)
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

    public override object[] GetInitData(Player player) => [Activated ? 1 : 0];

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

        var entityComponents = Room.Entities[Id];

        foreach (var component in entityComponents)
            if (component is IRecieverTriggered recieveable)
                recieveable.RecievedTrigger(activated);

        SendTriggerState(activated);
    }

    public void SendTriggerState(bool activated) =>
        Room.SendSyncEvent(new TriggerReceiver_SyncEvent(Id.ToString(), Room.Time, "now", activated, 0));
}
