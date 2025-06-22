using static A2m.Server.ExtLevelEditor;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

public interface IAIState
{
    void SetStateMachine(IAIStateMachine machine);

    string StateName { get; }

    void StartState();
    void UpdateState();
    void StopState();

    ComponentSettings GetFullSettings();
}
