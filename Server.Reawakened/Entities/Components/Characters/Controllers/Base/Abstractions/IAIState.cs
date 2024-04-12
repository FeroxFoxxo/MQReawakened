using static A2m.Server.ExtLevelEditor;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

public interface IAIState
{
    public IAIStateMachine AIStateMachine { set; }

    public string StateName { get; }

    public void StartState();
    public void UpdateState();
    public void StopState();

    public ComponentSettings GetFullSettings();
}
