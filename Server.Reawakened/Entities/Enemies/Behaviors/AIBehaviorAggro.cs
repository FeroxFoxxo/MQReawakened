using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;
using Server.Reawakened.XMLs.Data.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorAggro(AggroState aggroState, AIStatsGlobalComp globalComp) : AIBaseBehavior
{
    public float AggroSpeed => globalComp.Aggro_AttackSpeed != globalComp.Default.Aggro_AttackSpeed ? globalComp.Aggro_AttackSpeed : aggroState.AggroSpeed;
    public float MoveBeyondTargetDistance => globalComp.Aggro_MoveBeyondTargetDistance != globalComp.Default.Aggro_MoveBeyondTargetDistance ? globalComp.Aggro_MoveBeyondTargetDistance : aggroState.MoveBeyondTargetDistance;
    public bool StayOnPatrolPath => globalComp.Aggro_StayOnPatrolPath != globalComp.Default.Aggro_StayOnPatrolPath ? globalComp.Aggro_StayOnPatrolPath : aggroState.StayOnPatrolPath;
    public float AttackBeyondPatrolLine => globalComp.Aggro_AttackBeyondPatrolLine != globalComp.Default.Aggro_AttackBeyondPatrolLine ? globalComp.Aggro_AttackBeyondPatrolLine : aggroState.AttackBeyondPatrolLine;
    public bool UseAttackBeyondPatrolLine => aggroState.UseAttackBeyondPatrolLine;
    public float DetectionRangeUpY => aggroState.DetectionRangeUpY;
    public float DetectionRangeDownY => aggroState.DetectionRangeDownY;

    public override bool ShouldDetectPlayers => false;

    protected override AI_Behavior GetBehaviour() => new AI_Behavior_Aggro(
        AggroSpeed, MoveBeyondTargetDistance,
        StayOnPatrolPath, AttackBeyondPatrolLine,
        DetectionRangeUpY, DetectionRangeDownY
    );

    public override object[] GetData() => [
            AggroSpeed, MoveBeyondTargetDistance, StayOnPatrolPath,
            AttackBeyondPatrolLine, UseAttackBeyondPatrolLine,
            DetectionRangeUpY, DetectionRangeDownY
        ];

    public override void NextState(BehaviorEnemy enemy) =>
        enemy.ChangeBehavior(
            enemy.GenericScript.AwareBehavior,
            enemy.GenericScript.UnawareBehavior == StateType.ComeBack ? enemy.Position.x : enemy.AiData.Sync_TargetPosX,
            enemy.GenericScript.UnawareBehavior == StateType.ComeBack ? enemy.Position.y : enemy.AiData.Sync_TargetPosY,
            enemy.AiData.Intern_Dir
        );
}
