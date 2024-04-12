using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Enemies.Models;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Controller;
public abstract class BaseAIStateMachine<T> : Component<T>
{
    public IAIState[] CurrentStates = [];
    public List<IAIState> NextStates = [];

    public void AddNextState<AiState>() where AiState : class, IAIState =>
        NextStates.Add(Room.GetEntityFromId<AiState>(Id));

    public void GoToNextState()
    {
        if (Room == null || Room.IsObjectKilled(Id))
            return;

        foreach (var state in CurrentStates)
            state.StopState();

        var syncEvent2 = new AiStateSyncEvent()
        {
            InStates = PreviousState,
            GoToStates = NewState
        };

        CurrentStates = NextStates.ToArray();
        NextStates.Clear();

        foreach (var state in CurrentStates)
            state.StartState();

        Room.SendSyncEvent(syncEvent2.GetSyncEvent(Id, Room));
    }

    public override void Update()
    {
        foreach (var state in CurrentStates)
            state.UpdateState();
    }
}
