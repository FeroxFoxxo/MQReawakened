using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.XMLs.Models.Enemy.States;

public class GrenadierState(float gInTime, float gLoopTime, float gOutTime, bool isTracking,
    int projCount, float projSpeed, float maxHeight, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float GInTime { get; } = gInTime;
    public float GLoopTime { get; } = gLoopTime;
    public float GOutTime { get; } = gOutTime;
    public bool IsTracking { get; } = isTracking;
    public int ProjCount { get; } = projCount;
    public float ProjSpeed { get; } = projSpeed;
    public float MaxHeight { get; } = maxHeight;

    public override AIBaseBehavior CreateBaseBehaviour(AIStatsGenericComp generic) => new AIBehaviorGrenadier(this);

    public override string[] GetStartArgs(BehaviorEnemy behaviorEnemy) => [];

    public override string ToStateString(AIStatsGenericComp generic)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(GInTime);
        sb.Append(GLoopTime);
        sb.Append(GOutTime);
        sb.Append(IsTracking ? 1 : 0);
        sb.Append(ProjCount);
        sb.Append(ProjSpeed);
        sb.Append(MaxHeight);

        return sb.ToString();
    }
}
