using Server.Reawakened.Rooms.Models.Entities;
using static A2m.Server.ExtLevelEditor;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
public abstract class BaseAIState<T> : Component<T>, IAIState
{
    private IAIStateMachine internalStateMachine;

    public abstract string StateName { get; }

    IAIStateMachine IAIState.AIStateMachine { set => internalStateMachine = value; }

    public float StartTime = 0;

    public virtual void StartState() { }
    public virtual void UpdateState() { }
    public virtual void StopState() { }

    public void AddNextState<AiState>() where AiState : class, IAIState =>
        internalStateMachine.AddNextState<AiState>();

    public void GoToNextState() =>
        internalStateMachine.GoToNextState();

    public virtual ComponentSettings GetSettings() => [];
    private ComponentSettings GetStartSettings() => ["ST", StartTime.ToString()];

    public ComponentSettings GetFullSettings()
    {
        var startSettings = GetStartSettings();
        startSettings.AddRange(GetStartSettings());
        return startSettings;
    }
}
