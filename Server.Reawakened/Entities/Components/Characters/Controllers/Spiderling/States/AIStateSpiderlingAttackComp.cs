using A2m.Server;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.Spiderling.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Spiderling.States;
public class AIStateSpiderlingAttackComp : BaseAIState<AIStateSpiderlingAttackMQR>
{
    public override string StateName => "AIStateSpiderlingAttack";

    public float ShotInterval => ComponentData.ShotInterval;
    public float ShootTime => ComponentData.ShootTime;
    public float ShootDelay => ComponentData.ShootDelay;
    public string Projectile => ComponentData.Projectile;
    public float ProjectileSpeed => ComponentData.ProjectileSpeed;
    public float FirstProjectileAngleOffset => ComponentData.FirstProjectileAngleOffset;
    public int NumberOfProjectiles => ComponentData.NumberOfProjectiles;
    public float AngleBetweenProjectiles => ComponentData.AngleBetweenProjectiles;

    // Provide ForceDirectionX
    public override ExtLevelEditor.ComponentSettings GetSettings() => throw new NotImplementedException();
}
