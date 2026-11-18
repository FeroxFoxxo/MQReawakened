using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.Entities.Enemies.Models;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using static A2m.Server.ExtLevelEditor;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
public abstract class BaseAIStateMachine<T> : Component<T>, IAIStateMachine
{
    public IAIState CurrentState;
    public IAIState NextState;
    public List<IAIState> InactiveStates = [];

    public ILogger<T> Logger { get; set; }

    public AIStateEnemy EnemyData;
    public int ForceDirectionX;

    public void SetAIStateEnemy(AIStateEnemy enemy)
    {
        EnemyData = enemy;

        foreach (var state in Room.GetEntitiesFromId<IAIState>(Id))
            state.SetEnemyController(enemy);
    }

    public override void InitializeComponent()
    {
        foreach (var state in Room.GetEntitiesFromId<IAIState>(Id))
            state.SetStateMachine(this);

        ForceDirectionX = 0;
    }

    public void AddNextState<AiState>() where AiState : class, IAIState
    {
        var state = Room.GetEntityFromId<AiState>(Id);

        if (state == null)
        {
            var stateName = typeof(AiState).Name;
            var types = string.Join(", ", Room.GetEntitiesFromId<IAIState>(Id).Select(x => x.StateName));

            Logger.LogError(
                "Could not find state of {StateName} for {PrefabName}. Possible types: {Types}",
                stateName, PrefabName, types
            );

            return;
        }

        if (InactiveStates.Contains(state))
        {
            NextState = InactiveStates.First(x => x == state);
            return;
        }

        InactiveStates.Add(state);
        NextState = state;
    }

    public void AddNextState(Type t)
    {
        var state = Room.GetEntityFromId(Id, t);

        if (state == null)
        {
            var stateName = t.Name;
            var types = string.Join(", ", Room.GetEntitiesFromId<IAIState>(Id).Select(x => x.StateName));

            Logger.LogError(
                "Could not find state of {StateName} for {PrefabName}. Possible types: {Types}",
                stateName, PrefabName, types
            );

            return;
        }

        if (state is not IAIState aiState)
        {
            Logger.LogError(
                "Type {Type} for {PrefabName} is not an AI state.",
                t.Name, PrefabName
            );
            return;
        }

        if (InactiveStates.Contains(aiState))
        {
            NextState = InactiveStates.First(x => x == aiState);
            return;
        }

        InactiveStates.Add(aiState);
        NextState = aiState;
    }

    public void GoToNextState()
    {
        if (Room == null || Room.IsObjectKilled(Id) || NextState == null)
            return;

        if (CurrentState != null)
        {
            Logger.LogTrace(
                "Stopping state '{State}' for '{Name} ({Id})'",
                CurrentState.StateName, PrefabName, Id
            );

            CurrentState.StopState();
        }
        
        var time = Room.Time;

        var syncEvent2 = new AiStateSyncEvent()
        {
            InStates = GetGameComponents(CurrentState, 0f),
            GoToStates = GetGameComponents(NextState, time)
        };

        CurrentState = NextState;
        NextState = null;
        
        Logger.LogTrace(
            "Starting state '{State}' for '{Name} ({Id})'",
            CurrentState.StateName, PrefabName, Id
        );
        
        CurrentState.StartState(time);

        Room.SendSyncEvent(syncEvent2.GetSyncEvent(Id, time));
    }

    private static GameObjectComponents GetGameComponents(IAIState state, float time)
    {
        var components = new GameObjectComponents();

        if (state != null)
            components.Add(state.StateName, state.GetFullSettings(time));

        return components;
    }

    public override void Update()
    {
        if (Room == null)
            return;

        CurrentState?.UpdateState();
    }

    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player) { }

    public int GetForceDirectionX() => ForceDirectionX;
    public void SetForceDirectionX(int directionX) => ForceDirectionX = directionX;
}
