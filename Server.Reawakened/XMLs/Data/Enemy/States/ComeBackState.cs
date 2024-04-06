using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Entities.Enemies.Behaviors;
using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Abstractions;
using Server.Reawakened.XMLs.Data.Enemy.Models;

namespace Server.Reawakened.XMLs.Data.Enemy.States;
public class ComeBackState(float comeBackSpeed, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float ComeBackSpeed => comeBackSpeed;

    protected override AIBaseBehavior GetBaseBehaviour(AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) => new AIBehaviorComeBack(this, globalComp);

    public override string[] GetStartArgs(BehaviorEnemy behaviorEnemy) =>
        [
            behaviorEnemy.Position.x.ToString(),
            behaviorEnemy.AiData.Intern_SpawnPosY.ToString()
        ];
}
