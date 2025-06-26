using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorAggro(BehaviorEnemy enemy, AggroProperties fallback) : AIBaseBehavior(enemy.AiData, enemy.Room)
{
    public override bool ShouldDetectPlayers => false;

    public override AiProperties GetProperties() =>
        new AggroProperties(
            Fallback(enemy.Global.Aggro_AttackSpeed, fallback.aggro_AttackSpeed),
            Fallback(enemy.Global.Aggro_MoveBeyondTargetDistance, fallback.aggro_MoveBeyondTargetDistance),
            Fallback(enemy.Global.Aggro_StayOnPatrolPath, fallback.aggro_StayOnPatrolPath),
            Fallback(enemy.Global.Aggro_AttackBeyondPatrolLine, fallback.aggro_AttackBeyondPatrolLine),
            Fallback(enemy.Generic.Aggro_UseAttackBeyondPatrolLine, fallback.aggro_UseAttackBeyondPatrolLine),
            fallback.aggro_DetectionRangeUpY,
            fallback.aggro_DetectionRangeDownY
        );

    public override object[] GetStartArgs() => [];

    public override StateType GetStateType() => StateType.Aggro;

    public override void NextState() =>
        enemy.ChangeBehavior(
            enemy.Global.AwareBehavior,
            enemy.Global.UnawareBehavior == StateType.ComeBack ? enemy.Position.x : _aiData.Sync_TargetPosX,
            enemy.Global.UnawareBehavior == StateType.ComeBack ? enemy.Position.y : _aiData.Sync_TargetPosY,
            _aiData.Intern_Dir
        );
}
