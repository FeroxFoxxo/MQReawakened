using Server.Reawakened.Entities.Enemies.EnemyTypes;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
public interface IAIStateMachine
{
    void SetAIStateEnemy(AIStateEnemy enemy);
    AIStateEnemy GetAiStateEnemy();

    void AddNextState<AiState>() where AiState : class, IAIState;
    void AddNextState(Type t);
    void GoToNextState();
}
