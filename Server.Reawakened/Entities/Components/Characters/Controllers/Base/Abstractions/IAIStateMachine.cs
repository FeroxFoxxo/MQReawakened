namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
public interface IAIStateMachine
{
    public void AddNextState<AiState>() where AiState : class, IAIState;
    public void GoToNextState();
}
