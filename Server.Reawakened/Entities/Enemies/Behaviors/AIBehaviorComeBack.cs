using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorComeBack(ComeBackProperties properties, BehaviorEnemy enemy, StateType state) : AIBaseBehavior(enemy, state)
{
    public override bool ShouldDetectPlayers => true;

    public override AiProperties GetProperties() =>
        new ComeBackProperties(
            Enemy.Global.ComeBack_MoveSpeed != Enemy.Global.Default.ComeBack_MoveSpeed ? Enemy.Global.ComeBack_MoveSpeed : properties.comeBack_MoveSpeed
        );

    public override object[] GetStartArgs() => [Enemy.Position.x, Enemy.AiData.Intern_SpawnPosY];

    public override void NextState() =>
        Enemy.ChangeBehavior(StateType.Patrol, Enemy.Position.x, Enemy.Position.y, Enemy.AiData.Intern_Dir);
}
