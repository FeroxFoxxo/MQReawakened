using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Entities;
using UnityEngine;
using static A2m.Server.ExtLevelEditor;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
public abstract class BaseAIState<T, T2> : Component<T>, IAIState where T2 : class, IState
{
    public abstract string StateName { get; }

    public ILogger<T> Logger { get; set; }

    public bool StateActivated => _stateStartTime != 0f;
    public T2 State => _state as T2;

    public IAIStateMachine StateMachine;
    public AIStateEnemy EnemyController;

    protected float _stateStartTime;
    protected IState _state;

    // Abstract methods

    public abstract T2 GetInitialAIState();

    public virtual ComponentSettings GetSettings() => [];

    public virtual void StateIn() => Logger.LogTrace("Entering AI state: {StateName} for enemy: {Prefab}", StateName, PrefabName);

    public virtual void StateOut() => Logger.LogTrace("Exiting AI state: {StateName} for enemy: {Prefab}", StateName, PrefabName);

    public virtual void OnAIStateIn() => Logger.LogTrace("AI state in: {StateName} for enemy: {Prefab}", StateName, PrefabName);

    public virtual void OnAIStateOut() => Logger.LogTrace("AI state out: {StateName} for enemy: {Prefab}", StateName, PrefabName);

    public virtual void Execute()
    {
    }

    // Related state machine methods

    private void SetStateStartTime(float startTime)
    {
        if (_stateStartTime != startTime)
        {
            _stateStartTime = startTime;

            if (_stateStartTime == 0f)
                OnAIStateOut();
            else
                OnAIStateIn();
        }

        if (_state != null)
        {
            if (_state.StartTime == 0f && startTime != 0f)
            {
                _state.StartTime = startTime;
                _state.In();
            }
            else if (_state.StartTime != 0f && startTime == 0f)
            {
                _state.StartTime = startTime;
                _state.Out();
            }
        }
    }

    public void StartState() => SetStateStartTime(Room.Time);

    public void UpdateState()
    {
        if (_state != null)
        {
            _state.Execute(Room.Time);

            if (_state.Activated)
                Execute();
        }
        else if (_stateStartTime != 0f)
            Execute();
    }

    public void StopState() => SetStateStartTime(0);

    // Internal state machine methods

    public override void InitializeComponent()
    {
        _state = GetInitialAIState();

        if (_state is AI_State aiState)
            aiState.Observers += this;
        else if (_state is AI_State_Move moveState)
            moveState.Observers += this;
        else if (_state is AI_State_TrollBase trollState)
            trollState.Observers += this;
    }

    public void SetStateMachine(IAIStateMachine machine) => StateMachine = machine;

    public void SetEnemyController(AIStateEnemy enemyController) => EnemyController = enemyController;

    private ComponentSettings GetStartSettings() => ["ST", _stateStartTime.ToString()];

    public ComponentSettings GetFullSettings()
    {
        var startSettings = GetStartSettings();
        startSettings.AddRange(GetSettings());
        return startSettings;
    }

    public void AddNextState<AiState>() where AiState : class, IAIState =>
        StateMachine.AddNextState<AiState>();

    public void AddNextState(Type t) =>
        StateMachine.AddNextState(t);

    public void GoToNextState() =>
        StateMachine.GoToNextState();

    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player) { }

    // Useful methods

    public Vector3 GetDirectionToPlayer(Player player)
    {
        var spikerPosition = Position.ToUnityVector3();
        var playerPosition = player.TempData.Position;

        return (playerPosition - spikerPosition).normalized;
    }
}
