using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;

public class AIBehaviorStinger(StingerState stingerState) : AIBaseBehavior
{
    public AI_Behavior_Stinger StingerBehavior = new(
        stingerState.SpeedForward, stingerState.SpeedBackward,
        stingerState.InDurationForward, stingerState.AttackDuration,
        stingerState.DamageAttackTimeOffset, stingerState.InDurationBackward
    );

    public override void Start(ref AIProcessData aiData, float roomTime, string[] args) => StingerBehavior.Start(aiData, roomTime, args);

    public override bool Update(ref AIProcessData aiData, float roomTime) => StingerBehavior.Update(aiData, roomTime);

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime) => StingerBehavior.GetBehaviorRatio(aiData, roomTime);

    public override StateTypes GetBehavior() => StateTypes.Stinger;
}
