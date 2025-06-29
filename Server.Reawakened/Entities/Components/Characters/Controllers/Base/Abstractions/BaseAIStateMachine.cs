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
    public IAIState[] CurrentStates = [];
    public List<IAIState> NextStates = [];

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

        NextStates.Add(state);
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

        NextStates.Add(aiState);
    }

    public void GoToNextState()
    {
        if (Room == null || Room.IsObjectKilled(Id))
            return;

        foreach (var state in CurrentStates)
        {
            Logger.LogTrace(
                "Stopping state '{State}' for '{Name} ({Id})'",
                state.StateName, PrefabName, Id
            );

            state.StopState();
        }

        var syncEvent2 = new AiStateSyncEvent()
        {
            InStates = GetGameComponents(CurrentStates),
            GoToStates = GetGameComponents(NextStates)
        };

        CurrentStates = [.. NextStates];
        NextStates.Clear();

        foreach (var state in CurrentStates)
        {
            Logger.LogTrace(
                "Starting state '{State}' for '{Name} ({Id})'",
                state.StateName, PrefabName, Id
            );
            state.StartState();
        }

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

    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player) { }

    public int GetForceDirectionX() => ForceDirectionX;
    public void SetForceDirectionX(int directionX) => ForceDirectionX = directionX;
}
