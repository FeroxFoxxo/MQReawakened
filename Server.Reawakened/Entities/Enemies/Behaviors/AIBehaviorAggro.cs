using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorAggro(AggroProperties properties, BehaviorEnemy enemy, StateType state) : AIBaseBehavior(enemy, state)
{
    public override bool ShouldDetectPlayers => false;

    public override AiProperties GetProperties() =>
        new AggroProperties(
            Enemy.Global.Aggro_AttackSpeed != Enemy.Global.Default.Aggro_AttackSpeed ? Enemy.Global.Aggro_AttackSpeed : properties.aggro_AttackSpeed,
            Enemy.Global.Aggro_MoveBeyondTargetDistance != Enemy.Global.Default.Aggro_MoveBeyondTargetDistance ? Enemy.Global.Aggro_MoveBeyondTargetDistance : properties.aggro_MoveBeyondTargetDistance,
            Enemy.Global.Aggro_StayOnPatrolPath != Enemy.Global.Default.Aggro_StayOnPatrolPath ? Enemy.Global.Aggro_StayOnPatrolPath : properties.aggro_StayOnPatrolPath,
            Enemy.Global.Aggro_AttackBeyondPatrolLine != Enemy.Global.Default.Aggro_AttackBeyondPatrolLine ? Enemy.Global.Aggro_AttackBeyondPatrolLine : properties.aggro_AttackBeyondPatrolLine,
            properties.aggro_UseAttackBeyondPatrolLine,
            properties.aggro_DetectionRangeUpY,
            properties.aggro_DetectionRangeDownY
        );

    public override object[] GetStartArgs() => [];

    public override void NextState() =>
        Enemy.ChangeBehavior(
            Enemy.GenericScript.AwareBehavior,
            Enemy.GenericScript.UnawareBehavior == StateType.ComeBack ? Enemy.Position.x : Enemy.AiData.Sync_TargetPosX,
            Enemy.GenericScript.UnawareBehavior == StateType.ComeBack ? Enemy.Position.y : Enemy.AiData.Sync_TargetPosY,
            Enemy.AiData.Intern_Dir
        );
}
