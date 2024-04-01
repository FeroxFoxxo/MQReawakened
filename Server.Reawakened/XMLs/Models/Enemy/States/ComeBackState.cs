using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.XMLs.Models.Enemy.States;
public class ComeBackState(float comeBackSpeed, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float ComeBackSpeed { get; } = comeBackSpeed;

    public override AIBaseBehavior CreateBaseBehaviour(AIStatsGenericComp generic) => new AIBehaviorComeBack(this);

    public override string[] GetStartArgs(BehaviorEnemy behaviorEnemy) =>
        [
            behaviorEnemy.Position.x.ToString(),
            behaviorEnemy.AiData.Intern_SpawnPosY.ToString()
        ];

    public override string ToStateString(AIStatsGenericComp generic)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(ComeBackSpeed);

        return sb.ToString();
    }
}
