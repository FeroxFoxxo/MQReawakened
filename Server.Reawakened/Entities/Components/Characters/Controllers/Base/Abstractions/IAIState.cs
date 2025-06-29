using Server.Reawakened.Entities.Enemies.EnemyTypes;
using static A2m.Server.ExtLevelEditor;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

public interface IAIState
{
    string StateName { get; }

    void StartState();
    void UpdateState();
    void StopState();

    ComponentSettings GetFullSettings();
    void SetStateMachine(IAIStateMachine machine);
    void SetEnemyController(AIStateEnemy enemyController);
}
