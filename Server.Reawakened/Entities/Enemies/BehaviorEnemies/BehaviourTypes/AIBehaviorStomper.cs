using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;

public class AIBehaviorStomper(StomperState stomperState) : AIBaseBehavior
{
    public AI_Behavior_Stomper StomperBehavior = new(stomperState.AttackTime, stomperState.ImpactTime);

    public override void Start(ref AIProcessData aiData, float roomTime, string[] args) => StomperBehavior.Start(aiData, roomTime, args);

    public override bool Update(ref AIProcessData aiData, float roomTime) => StomperBehavior.Update(aiData, roomTime);

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime) => StomperBehavior.GetBehaviorRatio(aiData, roomTime);

    public override StateTypes GetBehavior() => StateTypes.Stomper;
}
