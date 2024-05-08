using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Entities;
using static A2m.Server.ExtLevelEditor;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
public abstract class BaseAIState<T> : Component<T>, IAIState
{
    public IAIStateMachine StateMachine;

    public abstract string StateName { get; }

    public float StartTime = 0;

    public virtual void StartState() { }
    public virtual void UpdateState() { }
    public virtual void StopState() { }

    public virtual ComponentSettings GetSettings() => [];

    public void AddNextState<AiState>() where AiState : class, IAIState =>
        StateMachine.AddNextState<AiState>();

    public void GoToNextState() =>
        StateMachine.GoToNextState();

    private ComponentSettings GetStartSettings() => ["ST", StartTime.ToString()];

    public ComponentSettings GetFullSettings()
    {
        var startSettings = GetStartSettings();
        startSettings.AddRange(GetSettings());
        return startSettings;
    }

    public void SetStateMachine(IAIStateMachine machine) => StateMachine = machine;

    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player) { }
}
