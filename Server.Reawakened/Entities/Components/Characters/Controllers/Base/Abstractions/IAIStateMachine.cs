using Server.Reawakened.Entities.Enemies.EnemyTypes;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
public interface IAIStateMachine
{
    public void SetAIStateEnemy(AIStateEnemy enemy);

    public void AddNextState<AiState>() where AiState : class, IAIState;
    public void GoToNextState();
}
