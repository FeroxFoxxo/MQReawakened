using Server.Reawakened.Entities.Enemies.Behaviors;
using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Abstractions;
using Server.Reawakened.XMLs.Data.Enemy.Models;

namespace Server.Reawakened.XMLs.Data.Enemy.States;

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

    public override AIBaseBehavior GetBaseBehaviour(BehaviorEnemy enemy) =>
        new AIBehaviorGrenadier(this, enemy);
}
