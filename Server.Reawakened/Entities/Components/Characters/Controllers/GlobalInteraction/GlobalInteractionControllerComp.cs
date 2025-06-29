using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.GlobalInteraction.States;
using Server.Reawakened.Entities.Components.GameObjects.Interactables;
using Server.Reawakened.Entities.Enemies.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Data.Enemy.Abstractions;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.GlobalInteraction;

public class GlobalInteractionControllerComp : BaseAIStateMachine<GlobalInteractionController>
{
    /* 
     * -- AI STATES --
     * AIStateGlobalInteractionActive
     * AIStateGlobalInteractionDone
     * AIStateGlobalInteractionInactive
    */

    public int MinimumRequiredInteractions => ComponentData.MinimumRequiredInteractions;
    public float RequiredInteractionsMultiplier => ComponentData.RequiredInteractionsMultiplier;
    public int BananasAwarded => ComponentData.BananasAwarded;
    public float PartyDuration => ComponentData.PartyDuration;
    public float PollInterval => ComponentData.PollInterval;
    public string TimedEventName => ComponentData.TimedEventName;

    private GlobalCounterInteractableControllerComp _counter;
    private float _nextPollTime;

    public override void InitializeComponent()
    {
        base.InitializeComponent();

        _nextPollTime = 0;

        var counter = Room.GetEntityFromId<GlobalCounterInteractableControllerComp>(Id);

        if (counter != null)
            _counter = counter;

        var defaultProperties = AISyncEventHelper.CreateDefaultGlobalProperties();
        var behaviors = new Dictionary<StateType, BaseState>();

        Room.SendSyncEvent(
            AISyncEventHelper.AIInit(
                Id, Room,
                Position.X, Position.Y, Position.Z, Position.X, Position.Y,
                0, 10, 10, 1, 1, 1, 0, 1, defaultProperties, behaviors
            )
        );

        AddNextState<AIStateGlobalInteractionActiveComp>();
        GoToNextState();
    }

    public override void Update()
    {
        if (Room.Time >= _nextPollTime)
        {
            var pollStatus = new SyncEvent(Id, SyncEvent.EventType.GlobalInteractionEvent, Room.Time);
            pollStatus.EventDataList.Add(_counter.Interactions);
            Room.SendSyncEvent(pollStatus);

            _nextPollTime = Room.Time + PollInterval;
        }
    }
}
