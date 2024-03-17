using Microsoft.Extensions.Logging;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.AIStates.SyncEvents;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Entity.Utils;
using Server.Reawakened.Entities.Entity;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using static A2m.Server.ExtLevelEditor;
using Server.Reawakened.Entities.AbstractComponents;
using A2m.Server;
using Server.Reawakened.Rooms.Models.Planes;
using Microsoft.Extensions.DependencyInjection;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Entities.AIStates;
public class AIStatePatrolComp : Component<AIStatePatrol>, IDamageable
{
    public Vector2Model Patrol1 => new()
    {
        X = ComponentData.Patrol1.x,
        Y = ComponentData.Patrol1.y
    };

    public Vector2Model Patrol2 => new()
    {
        X = ComponentData.Patrol2.x,
        Y = ComponentData.Patrol2.y
    };
    public int SinusPathNbHalfPeriod => ComponentData.SinusPathNbHalfPeriod;
    public float MovementSpeed => ComponentData.MovementSpeed;
    public float IdleDurationAtTurnAround => ComponentData.IdleDurationAtTurnAround;
    public float DetectionRange => ComponentData.DetectionRange;
    public float MinimumRange => ComponentData.MinimumRange;
    public float MinimumTimeBeforeDetection => ComponentData.MinimumTimeBeforeDetection;
    public float MaximumYDifferenceOnDetection => ComponentData.MaximumYDifferenceOnDetection;
    public bool RayCastDetection => ComponentData.RayCastDetection;
    public bool DetectOnlyInPatrolZone => ComponentData.DetectOnlyInPatrolZone;
    public float PatrolZoneSizeOffset => ComponentData.PatrolZoneSizeOffset;

    public string IdOfAttackingEnemy;
    public bool IsAttacking;

    public int MaxHealth { get; set; }
    public int CurrentHealth { get; set; }
    public Vector3Model StartingPostion = new();

    public GameObjectComponents PreviousState = [];
    public ItemCatalog ItemCatalog { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public TimerThread TimerThread { get; set; }
    public IServiceProvider Service { get; set; }
    public ILogger<AIStatePatrolComp> Logger { get; set; }

    public override void InitializeComponent() => RunPlacement();

    public override void Update()
    {
        var closestPlayer = GetClosestTarget();
        if (closestPlayer == null) return;

        //Magic numbers here are used to store the width and height of the enemy colliders.
        var detectionRangeCollider = new EnemyCollider(Id, Position, 8, 4, ParentPlane, Room);
        var isColliding = detectionRangeCollider.CheckCollision(new PlayerCollider(closestPlayer));

        if (isColliding)
        {
            IdOfAttackingEnemy = Id;

            foreach (var drake in Room.GetEntitiesFromType<DrakeEnemyControllerComp>()
                .Where(x => x.Id == IdOfAttackingEnemy))
                RunMeleeAttackState(null);

            //Not using spiker attack code yet.
            foreach (var spiker in Room.GetEntitiesFromType<AIStatePatrolComp>()
                .Where(x => x.Id == IdOfAttackingEnemy))
                RunProjectileAttackState(null);
        }
    }

    public void RunPlacement()
    {
        StartingPostion = Position;

        var editedPosition = new Vector3Model()
        {
            X = Position.X += Rectangle.X,
            Y = Position.Y += Rectangle.Y,
            Z = Position.Z
        };

        Room.Colliders.Add(Id, new EnemyCollider(Id, editedPosition,
            Rectangle.Width, Rectangle.Height, ParentPlane, Room));

        var nextState = new GameObjectComponents() {
            {"AIStateDrakePlacement", new ComponentSettings()
                {
                 Position.X.ToString(),
                 Position.Y.ToString(),
                 Position.Z.ToString(),
                 Position.X.ToString(),
                 Position.Y.ToString(),
                 Position.Z.ToString()
                }
            }
        };

        GoToNextState(nextState);

        //Wait 1 second before running RunPatrol.
        TimerThread.DelayCall(RunPatrol, null, TimeSpan.FromSeconds(1), TimeSpan.Zero, 1);
    }

    public void RunPatrol(object _)
    {
        IsAttacking = false;

        var backPlaneZValue = ParentPlane == ServerRConfig.BackPlane ?
                      ServerRConfig.Planes[ServerRConfig.FrontPlane] :
                       ServerRConfig.Planes[ServerRConfig.BackPlane] ;

        var nextState = new GameObjectComponents() 
        {
            {"AIStatePatrol", new ComponentSettings()
                //Use Patrol1/Patrol2 values to adjust patrol positions. Else some enemies will phase through terrain.
                {Convert.ToString(StartingPostion.X + Patrol2.X),
                 StartingPostion.Y.ToString(),
                 backPlaneZValue.ToString()}
            }
        };

        GoToNextState(nextState);
    }

    public Player GetClosestTarget()
    {
        var distancesFromEnemy = new Dictionary<double, Player>();

        foreach (var player in Room.Players.Values)
        {
            var playerParentPlane = player.GetPlayersPlaneString();
            if (ParentPlane != playerParentPlane)
                return null;

            var playerPos = player.TempData.Position;
            var enemyPos = Position;

            var closestDistance = Math.Sqrt(Math.Pow(enemyPos.X - playerPos.X, 2) +
                                            Math.Pow(enemyPos.Y - playerPos.Y, 2));

            distancesFromEnemy.Add(closestDistance, player);
        }

        var closestPlayer = distancesFromEnemy.Keys.Min();

        return distancesFromEnemy[closestPlayer];
    }

    public void RunMeleeAttackState(object _)
    {
        if (IsAttacking || Id != IdOfAttackingEnemy.ToString())
            return;

        IsAttacking = true;

        var closestPlayerPos = GetClosestTarget().TempData.Position;

        var nextState = new GameObjectComponents()
            {
                {"AIStateDrakeAttack", new ComponentSettings()
                {Position.X.ToString(), Position.Y.ToString(), closestPlayerPos.Z.ToString(),
                 Position.X.ToString(), Position.Y.ToString(), closestPlayerPos.Z.ToString(),
                 closestPlayerPos.X.ToString(), closestPlayerPos.Y.ToString(), closestPlayerPos.Z.ToString()}}
            };

        GoToNextState(nextState);

        //Run patrol state after attack state/animation.
        TimerThread.DelayCall(RunPatrol, null, TimeSpan.FromSeconds(9), TimeSpan.Zero, 1);
    }

    public void RunProjectileAttackState(object _)
    {
        // this method is VERY buggy.

        if (IsAttacking || Id != IdOfAttackingEnemy) return;

        IsAttacking = true;

        var closestPlayer = GetClosestTarget();
        var nextState = new GameObjectComponents()
                    {
                        {"AIStateSpikerAttack", new ComponentSettings()
                            {
                                closestPlayer.TempData.Position.X.ToString(),
                            }
                        }
                    };

        var rand = new Random();
        var projectileId = Math.Abs(rand.Next()).ToString();

        while (Room.GameObjectIds.Contains(projectileId))
            projectileId = Math.Abs(rand.Next()).ToString();

        //Determines direction and speed of enemy projectile.
        var direction = closestPlayer.TempData.Position.X > Position.X ? 5 : -5;

        //Successfully sends enemy projectiles, however needs position placement adjustments based off of synced movement.
        Room.SendSyncEvent(AISyncEventHelper.AILaunchItem(Id, Room.Time,
            Position.X, Position.Y, Position.Z, direction, 0, 3, int.Parse(projectileId), 0));

        //Magic numbers here and everywhere else will be fixed when AIState gets merged with default enemies
        var aiProjectile = new AIProjectileEntity(Room, Id, projectileId, Position, 5, 1, 3, TimerThread, 1, ItemEffectType.BluntDamage, ItemCatalog);
        Room.Projectiles.Add(projectileId, aiProjectile);

        GoToNextState(nextState);

        //This method also seemingly creates position placement issues on running the patrol state afterwards.
        TimerThread.DelayCall(RunPatrol, null, TimeSpan.FromSeconds(3), TimeSpan.Zero, 1);
    }

    public void GoToNextState(GameObjectComponents NewState)
    {
        var syncEvent2 = new AiStateSyncEvent()
        {
            InStates = PreviousState,
            GoToStates = NewState
        };

        PreviousState = NewState;

        Room.SendSyncEvent(syncEvent2.GetSyncEvent(Id, Room));
    }

    public int GetDamageAmount(int damage, Elemental damageType)
    {
        var interObjStatus = Service.GetRequiredService<InterObjStatusComp>();

        switch (damageType)
        {
            case Elemental.Air:
                damage -= interObjStatus.AirDamageResistPoints;
                break;
            case Elemental.Fire:
                damage -= interObjStatus.FireDamageResistPoints;
                break;
            case Elemental.Ice:
                damage -= interObjStatus.IceDamageResistPoints;
                break;
            case Elemental.Earth:
                damage -= interObjStatus.EarthDamageResistPoints;
                break;
            case Elemental.Poison:
                damage -= interObjStatus.PoisonDamageResistPoints;
                break;
            case Elemental.Standard:
            case Elemental.Unknown:
            case Elemental.Invalid:
                damage -= interObjStatus.StandardDamageResistPoints;
                break;
        }

        if (damage < 0)
            damage = 0;

        return damage;
    }
}
