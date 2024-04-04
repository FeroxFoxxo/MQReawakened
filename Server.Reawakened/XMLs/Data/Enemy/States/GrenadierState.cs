using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.XMLs.Models.Enemy.States;

public class GrenadierState(float gInTime, float gLoopTime, float gOutTime, bool isTracking,
    int projCount, float projSpeed, float maxHeight, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float GInTime => gInTime;
    public float GLoopTime => gLoopTime;
    public float GOutTime => gOutTime;
    public bool IsTracking => isTracking;
    public int ProjCount => projCount;
    public float ProjSpeed => projSpeed;
    public float MaxHeight => maxHeight;

    protected override AIBaseBehavior GetBaseBehaviour(AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) => new AIBehaviorGrenadier(this);
}
