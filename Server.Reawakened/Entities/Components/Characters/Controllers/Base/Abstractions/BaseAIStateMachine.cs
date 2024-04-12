using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Enemies.Models;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using static A2m.Server.ExtLevelEditor;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Controller;
public abstract class BaseAIStateMachine<T> : Component<T>, IAIStateMachine
{
    public IAIState[] CurrentStates = [];
    public List<IAIState> NextStates = [];

    public void AddNextState<AiState>() where AiState : class, IAIState
    {
        var state = Room.GetEntityFromId<AiState>(Id) ?? throw new NullReferenceException();
        state.AIStateMachine = this;
        NextStates.Add(state);
    }

    public void GoToNextState()
    {
        if (Room == null || Room.IsObjectKilled(Id))
            return;

        foreach (var state in CurrentStates)
            state.StopState();

        var syncEvent2 = new AiStateSyncEvent()
        {
            InStates = GetGameComponents(CurrentStates),
            GoToStates = GetGameComponents(NextStates)
        };

        CurrentStates = [.. NextStates];
        NextStates.Clear();

        foreach (var state in CurrentStates)
            state.StartState();

        Room.SendSyncEvent(syncEvent2.GetSyncEvent(Id, Room));
    }

    private static GameObjectComponents GetGameComponents(IEnumerable<IAIState> states)
    {
        var components = new GameObjectComponents();

        foreach (var state in states)
            components.Add(state.StateName, state.GetFullSettings());

        return components;
    }

    public override void Update()
    {
        foreach (var state in CurrentStates)
            state.UpdateState();
    }
}
