using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorComeBack(BehaviorEnemy enemy, ComeBackProperties fallback) : AIBaseBehavior(enemy.AiData, enemy.Room)
{
    public override bool ShouldDetectPlayers => true;

    public override AiProperties GetProperties() =>
        new ComeBackProperties(
            Fallback(enemy.Global.ComeBack_MoveSpeed, fallback.comeBack_MoveSpeed)
        );

    public override object[] GetStartArgs() => [enemy.Position.x, _aiData.Intern_SpawnPosY];

    public override StateType GetStateType() => StateType.ComeBack;

    public override void NextState() =>
        enemy.ChangeBehavior(StateType.Patrol, enemy.Position.x, enemy.Position.y, _aiData.Intern_Dir);
}
