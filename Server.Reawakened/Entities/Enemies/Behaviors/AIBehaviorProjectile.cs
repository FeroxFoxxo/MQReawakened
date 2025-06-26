using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;
public class AIBehaviorProjectile(BehaviorEnemy enemy, ProjectileProperties fallback) : AIBaseBehavior(enemy.AiData, enemy.Room)
{
    public override bool ShouldDetectPlayers => false;

    public override AiProperties GetProperties() => fallback;

    public override object[] GetStartArgs() => [];

    public override StateType GetStateType() => StateType.Projectile;

    public override void NextState() =>
        enemy.ChangeBehavior(enemy.Global.AwareBehavior, _aiData.Sync_TargetPosX, _aiData.Sync_TargetPosY, _aiData.Intern_Dir);
}
