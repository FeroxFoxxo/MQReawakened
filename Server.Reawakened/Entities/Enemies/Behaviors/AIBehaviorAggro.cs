using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorAggro(AggroProperties properties, BehaviorEnemy enemy) : AIBaseBehavior(enemy)
{
    public float AggroSpeed => Enemy.Global.Aggro_AttackSpeed != Enemy.Global.Default.Aggro_AttackSpeed ? Enemy.Global.Aggro_AttackSpeed : properties.aggro_AttackSpeed;
    public float MoveBeyondTargetDistance => Enemy.Global.Aggro_MoveBeyondTargetDistance != Enemy.Global.Default.Aggro_MoveBeyondTargetDistance ? Enemy.Global.Aggro_MoveBeyondTargetDistance : properties.aggro_MoveBeyondTargetDistance;
    public bool StayOnPatrolPath => Enemy.Global.Aggro_StayOnPatrolPath != Enemy.Global.Default.Aggro_StayOnPatrolPath ? Enemy.Global.Aggro_StayOnPatrolPath : properties.aggro_StayOnPatrolPath;
    public float AttackBeyondPatrolLine => Enemy.Global.Aggro_AttackBeyondPatrolLine != Enemy.Global.Default.Aggro_AttackBeyondPatrolLine ? Enemy.Global.Aggro_AttackBeyondPatrolLine : properties.aggro_AttackBeyondPatrolLine;
    public bool UseAttackBeyondPatrolLine => properties.aggro_UseAttackBeyondPatrolLine;
    public float DetectionRangeUpY => properties.aggro_DetectionRangeUpY;
    public float DetectionRangeDownY => properties.aggro_DetectionRangeDownY;

    public override bool ShouldDetectPlayers => false;

    public override StateType State => StateType.Aggro;

    public override object[] GetProperties() => [
            AggroSpeed, MoveBeyondTargetDistance, StayOnPatrolPath,
            AttackBeyondPatrolLine, UseAttackBeyondPatrolLine,
            DetectionRangeUpY, DetectionRangeDownY
        ];

    public override object[] GetStartArgs() => [];

    public override void NextState() =>
        Enemy.ChangeBehavior(
            Enemy.GenericScript.AwareBehavior,
            Enemy.GenericScript.UnawareBehavior == StateType.ComeBack ? Enemy.Position.x : Enemy.AiData.Sync_TargetPosX,
            Enemy.GenericScript.UnawareBehavior == StateType.ComeBack ? Enemy.Position.y : Enemy.AiData.Sync_TargetPosY,
            Enemy.AiData.Intern_Dir
        );
}
