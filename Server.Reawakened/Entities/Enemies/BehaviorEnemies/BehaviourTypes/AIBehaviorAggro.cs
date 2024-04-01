using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;

public class AIBehaviorAggro(AggroState aggroState) : AIBaseBehavior
{
    public AI_Behavior_Aggro AggroBehavior = new(
        aggroState.AggroSpeed, aggroState.MoveBeyondTargetDistance,
        aggroState.StayOnPatrolPath, aggroState.AttackBeyondPatrolLine,
        aggroState.DetectionRangeUpY, aggroState.DetectionRangeDownY
    );

    public override void Start(ref AIProcessData aiData, float roomTime, string[] args) => AggroBehavior.Start(aiData, roomTime, args);

    public override bool Update(ref AIProcessData aiData, float roomTime) => AggroBehavior.Update(aiData, roomTime);

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime) => AggroBehavior.GetBehaviorRatio(aiData, roomTime);

    public override StateTypes GetBehavior() => StateTypes.Aggro;
}
