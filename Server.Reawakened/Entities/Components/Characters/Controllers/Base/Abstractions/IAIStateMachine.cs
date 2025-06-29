using Server.Reawakened.Entities.Enemies.EnemyTypes;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
public interface IAIStateMachine
{
    void AddNextState<AiState>() where AiState : class, IAIState;
    void AddNextState(Type t);
    void GoToNextState();

    void SetAIStateEnemy(AIStateEnemy enemy);

    int GetForceDirectionX();
    void SetForceDirectionX(int directionX);
}
