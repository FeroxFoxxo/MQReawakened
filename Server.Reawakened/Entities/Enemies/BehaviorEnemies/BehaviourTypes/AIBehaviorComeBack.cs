using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;

public class AIBehaviorComeBack(ComeBackState comeBackState) : AIBaseBehavior
{
    public AI_Behavior_ComeBack ComeBackBehavior = new(
        comeBackState.ComeBackSpeed
    );

    public override void Start(ref AIProcessData aiData, float startTime, string[] args) => ComeBackBehavior.Start(aiData, startTime, args);

    public override bool Update(ref AIProcessData aiData, float roomTime) => ComeBackBehavior.Update(aiData, roomTime);

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime) => ComeBackBehavior.GetBehaviorRatio(aiData, roomTime);

    public override bool MustDoComeback(AIProcessData aiData) => ComeBackBehavior.MustDoComeback(aiData, ComeBackBehavior);

    public override StateTypes GetBehavior() => StateTypes.ComeBack;
}
