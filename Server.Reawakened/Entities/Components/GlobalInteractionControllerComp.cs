using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Entities.AIStates.SyncEvents;
using static A2m.Server.ExtLevelEditor;

namespace Server.Reawakened.Entities.Components;

public class GlobalInteractionControllerComp : Component<GlobalInteractionController>
{
    public int MinimumRequiredInteractions => ComponentData.MinimumRequiredInteractions;
    public float RequiredInteractionsMultiplier => ComponentData.RequiredInteractionsMultiplier;
    public int BananasAwarded => ComponentData.BananasAwarded;
    public float PartyDuration => ComponentData.PartyDuration;
    public float PollInterval => ComponentData.PollInterval;
    public string TimedEventName => ComponentData.TimedEventName;

    public GameObjectComponents PreviousState = [];

    private GlobalCounterInteractableControllerComp _counter;
    private float _nextPollTime;

    public override void InitializeComponent()
    {
        _nextPollTime = 0;

        var counter = Room.GetEntityFromId<GlobalCounterInteractableControllerComp>(Id);

        if (counter != null)
            _counter = counter;

        var t = new AIInit_SyncEvent(Id, Room.Time, Position.X, Position.Y, Position.Z, Position.X, Position.Y, 0, 10, 10, 1, 1, 1, 0, 1, "", "");
        Room.SendSyncEvent(t);

        GoToNextState(new GameObjectComponents() {
                    {"AIStateGlobalInteractionActive", new ComponentSettings() {"ST", "0"}}
                });
    }

    public override void Update()
    {
        base.Update();
        if (Room.Time >= _nextPollTime)
        {
            var pollStatus = new SyncEvent(Id, SyncEvent.EventType.GlobalInteractionEvent, Room.Time);
            pollStatus.EventDataList.Add(_counter.Interactions);
            Room.SendSyncEvent(pollStatus);

            //var triggerCounter = new Trigger_SyncEvent(Id, Room.Time, true, Id, true);
            //Room.SendSyncEvent(triggerCounter);

            _nextPollTime = Room.Time + PollInterval;
        }
    }

    public void GoToNextState(GameObjectComponents NewState)
    {
        if (Room == null || Room.IsObjectKilled(Id))
            return;

        var syncEvent2 = new AiStateSyncEvent()
        {
            InStates = PreviousState,
            GoToStates = NewState
        };

        PreviousState = NewState;

        Room.SendSyncEvent(syncEvent2.GetSyncEvent(Id, Room));
    }
}
