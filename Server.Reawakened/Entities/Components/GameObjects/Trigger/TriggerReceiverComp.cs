using Discord;
using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Controller;
using Server.Reawakened.Entities.Components.GameObjects.Spawners;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Enums;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using System.Text;

namespace Server.Reawakened.Entities.Components.GameObjects.Trigger;

public class TriggerReceiverComp : Component<TriggerReceiver>, ICoopTriggered
{
    private int _activations;
    private int _deactivations;
    private string _triggeredBy;

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

    private TriggerReceiverCollider _collider;

    public override void InitializeComponent()
    {
        _collider = new TriggerReceiverCollider(Id, Position.ToUnityVector3(), Rectangle.ToRect(), ParentPlane, Room);
        if (CollisionType == TriggerReceiver.ReceiverCollisionType.Never)
            _collider.Active = false;
    }
    public override void DelayedComponentInitialization()
    {
        base.InitializeComponent();
        Trigger(ActiveByDefault, string.Empty);

        //This is placed in delayed init so that more important components take collider precedence
        Room.AddCollider(_collider);
    }

    public override void SendDelayedData(Player player) => player.SendSyncEventToPlayer(new TriggerReceiver_SyncEvent(Id, Room.Time, _triggeredBy, Activated, 0));

    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player) { }

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        if (syncEvent.Type != SyncEvent.EventType.TriggerReceiver)
            return;

        var tEvent = new TriggerReceiver_SyncEvent(syncEvent);

        Trigger(tEvent.Activate, player.GameObjectId);
    }

    public void TriggerStateChange(TriggerType triggerType, bool triggered, string triggeredBy)
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
            Trigger(true, triggeredBy);
        else if (_deactivations >= NbDeactivationsNeeded)
            Trigger(false, triggeredBy);

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

    public void Trigger(bool activated, string triggeredBy)
    {
        Activated = activated;
        _triggeredBy = triggeredBy;

        LogTriggerRecieved();

        foreach (var recieveable in Room.GetEntitiesFromId<IRecieverTriggered>(Id))
            recieveable.RecievedTrigger(activated);

        SendTriggerState(activated, triggeredBy);

        if (CollisionType == TriggerReceiver.ReceiverCollisionType.WhileActivate)
            _collider.Active = activated;
        if (CollisionType == TriggerReceiver.ReceiverCollisionType.WhileDeactivate)
            _collider.Active = !activated;
    }

    public void SendTriggerState(bool activated, string triggeredBy) =>
        Room.SendSyncEvent(new TriggerReceiver_SyncEvent(Id.ToString(), Room.Time, triggeredBy, activated, 0));
}
