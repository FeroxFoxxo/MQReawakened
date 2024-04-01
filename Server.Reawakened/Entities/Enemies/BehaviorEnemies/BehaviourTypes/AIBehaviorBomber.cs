using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;

public class AIBehaviorBomber(BomberState bomberState) : AIBaseBehavior
{
    public AI_Behavior_Bomber BomberBehavior = new(
        bomberState.InTime, bomberState.LoopTime
    );

    public override void Start(ref AIProcessData aiData, float roomTime, string[] args) => BomberBehavior.Start(aiData, roomTime, args);

    public override bool Update(ref AIProcessData aiData, float roomTime) => BomberBehavior.Update(aiData, roomTime);

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime) => BomberBehavior.GetBehaviorRatio(aiData, roomTime);

    public override StateTypes GetBehavior() => StateTypes.Bomber;
}
