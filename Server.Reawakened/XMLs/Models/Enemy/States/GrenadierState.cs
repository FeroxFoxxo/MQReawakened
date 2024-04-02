using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
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

    protected override AIBaseBehavior GetBaseBehaviour(AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) => new AIBehaviorGrenadier(this);
}
