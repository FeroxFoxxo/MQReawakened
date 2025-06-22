using A2m.Server;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
using Server.Reawakened.Entities.DataComponentAccessors.Spiker.States;
using Server.Reawakened.Rooms.Extensions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Spiker.States;
public class AIStateSpikerAttackComp : BaseAIState<AIStateSpikerAttackMQR>
{
    public override string StateName => "AIStateSpikerAttack";

    public float ShootTime => ComponentData.ShootTime;
    public float ProjectileTime => ComponentData.ProjectileTime;
    public string Projectile => ComponentData.Projectile;
    public float ProjectileSpeed => ComponentData.ProjectileSpeed;
    public float FirstProjectileAngleOffset => ComponentData.FirstProjectileAngleOffset;
    public int NumberOfProjectiles => ComponentData.NumberOfProjectiles;
    public float AngleBetweenProjectiles => ComponentData.AngleBetweenProjectiles;

    private int _forceDirectionX = 0;

    private AIStatePatrolComp _patrolComp;

    public override ExtLevelEditor.ComponentSettings GetSettings() => [_forceDirectionX.ToString()];

    public override void DelayedComponentInitialization() => _patrolComp = Room.GetEntityFromId<AIStatePatrolComp>(Id);

    public override void StartState()
    {
        var closestPlayer = Room.GetClosetPlayer(Position.ToUnityVector3(), float.MaxValue);

        if (closestPlayer == null)
        {
            _forceDirectionX = 0;
            return;
        }

        var playerPosition = closestPlayer.TempData.Position.x;
        var spikerPosition = Position.ToUnityVector3().x;

        _forceDirectionX = Convert.ToInt32(spikerPosition - playerPosition);
    }

    public override void UpdateState()
    {
        if (_patrolComp == null)
            return;

        var closestPlayer = _patrolComp.GetClosestPlayer();

        if (closestPlayer == null)
        {
            AddNextState<AIStatePatrolComp>();
            GoToNextState();
            return;
        }
    }
}
