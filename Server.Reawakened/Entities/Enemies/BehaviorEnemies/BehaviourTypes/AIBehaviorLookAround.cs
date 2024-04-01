using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;

public class AIBehaviorLookAround(LookAroundState lookAroundState) : AIBaseBehavior
{
    public AI_Behavior_LookAround LookAroundBehavior = new(
        lookAroundState.LookTime, lookAroundState.InitialProgressRatio,
        lookAroundState.SnapOnGround
    );

    public override void Start(ref AIProcessData aiData, float startTime, string[] args) => LookAroundBehavior.Start(aiData, startTime, args);

    public override bool Update(ref AIProcessData aiData, float roomTime) => LookAroundBehavior.Update(aiData, roomTime);

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime) => LookAroundBehavior.GetBehaviorRatio(aiData, roomTime);

    public override StateTypes GetBehavior() => StateTypes.LookAround;
}
