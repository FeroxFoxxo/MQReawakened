using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;

public class AIBehaviorAggro(AggroState aggroState, AIStatsGlobalComp globalComp) : AIBaseBehavior
{
    public float AggroSpeed => globalComp.Aggro_AttackSpeed != default ? globalComp.Aggro_AttackSpeed : aggroState.AggroSpeed;
    public float MoveBeyondTargetDistance => globalComp.Aggro_MoveBeyondTargetDistance != default ? globalComp.Aggro_MoveBeyondTargetDistance : aggroState.MoveBeyondTargetDistance;
    public bool StayOnPatrolPath => globalComp.Aggro_StayOnPatrolPath != default ? globalComp.Aggro_StayOnPatrolPath : aggroState.StayOnPatrolPath;
    public float AttackBeyondPatrolLine => globalComp.Aggro_AttackBeyondPatrolLine != default ? globalComp.Aggro_AttackBeyondPatrolLine : aggroState.AttackBeyondPatrolLine;
    public bool UseAttackBeyondPatrolLine => aggroState.UseAttackBeyondPatrolLine;
    public float DetectionRangeUpY => aggroState.DetectionRangeUpY;
    public float DetectionRangeDownY => aggroState.DetectionRangeDownY;

    public override float ResetTime => 0;

    protected override AI_Behavior GetBehaviour() => new AI_Behavior_Aggro(
        AggroSpeed, MoveBeyondTargetDistance,
        StayOnPatrolPath, AttackBeyondPatrolLine,
        DetectionRangeUpY, DetectionRangeDownY
    );

    public override StateTypes GetBehavior() => StateTypes.Aggro;

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(AggroSpeed);
        sb.Append(MoveBeyondTargetDistance);
        sb.Append(StayOnPatrolPath ? 1 : 0);
        sb.Append(AttackBeyondPatrolLine);
        sb.Append(UseAttackBeyondPatrolLine ? 1 : 0);
        sb.Append(DetectionRangeUpY);
        sb.Append(DetectionRangeDownY);

        return sb.ToString();
    }
}
