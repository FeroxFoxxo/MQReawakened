using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;
public class AIBehaviorSpike(BehaviorEnemy enemy, SpikeProperties fallback) : AIBaseBehavior(enemy.AiData, enemy.Room)
{
    public override bool ShouldDetectPlayers => false;
    public override bool ShouldAggroOnHit => false;

    public override AiProperties GetProperties() =>
        // These are "hardcoded", but they just match the data in the scripts
        new SpikeProperties
        (
            16,     // Travel In Speed
            8,      // Travel Out Speed
            0.7f,
            3.3f,
            1.34f,
            0.416f,
            15,     // Targetted Spread Angle
            16,     // Detection Range
            3,      // Targetted Projectile Count
            12,     // Random Projectile Count
            7       // 
        );

    private SpikeProperties GetInternalProperties() => fallback;

    public override object[] GetStartArgs(bool _)
    {
        var properties = GetInternalProperties();
        return [
            _aiData.Sync_TargetPosX,
            _aiData.Sync_TargetPosY,
            10,
            16,
            8
        ] ;
    }

    public override StateType GetStateType() => StateType.Spike;

    public override void NextState() =>
        enemy.ChangeBehavior(enemy.Global.AwareBehavior, enemy.Position.X, enemy.Position.Y, _aiData.Intern_Dir);
}
