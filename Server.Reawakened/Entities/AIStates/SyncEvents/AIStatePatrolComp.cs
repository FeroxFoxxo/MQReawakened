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
using Server.Reawakened.Rooms.Models.Planes;
using static A2m.Server.ExtLevelEditor;
using System.Runtime.CompilerServices;

namespace Server.Reawakened.Entities.AIStates;
public class AIStatePatrolComp : Component<AIStatePatrol>
{

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

    public ServerRConfig ServerRConfig { get; set; }
    public TimerThread TimerThread { get; set; }
    public ILogger<AIStatePatrolComp> Logger { get; set; }
    public GameObjectComponents PreviousState = [];
    public override void InitializeComponent()
    {
        RunPlacement();
        //TimerThread.DelayCall(SpikerTest, null, TimeSpan.FromSeconds(15), TimeSpan.Zero, 1);
    }

    public void RunPlacement()
    {
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

        TimerThread.DelayCall(RunPatrol, null, TimeSpan.FromSeconds(1), TimeSpan.Zero, 1);
    }

    public override void Update()
    {
        var closestPlayer = GetClosestTarget();

        if (closestPlayer == null) return;

        var detectionRangeCollider = new EnemyCollider(Id, Position, 8, 4, ParentPlane, Room);
        var isColliding = detectionRangeCollider.CheckCollision(new PlayerCollider(closestPlayer));

        if (isColliding)
        {
            IdOfAttackingEnemy = Id;

            foreach (var drake in Room.GetEntitiesFromType<DrakeEnemyControllerComp>()
                .Where(x => x.Id == IdOfAttackingEnemy))
                RunDragonAttackState(null);

            foreach (var spiker in Room.GetEntitiesFromType<AIStatePatrolComp>()
                .Where(x => x.Id == IdOfAttackingEnemy))
                RunSpikerAttackState(null);
        }
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

    public void RunPatrol(object _)
    {
        IsAttacking = false;

        var backPlaneZValue = ParentPlane == ServerRConfig.BackPlane ?
                       ServerRConfig.Planes[ServerRConfig.FrontPlane] :
                         ServerRConfig.Planes[ServerRConfig.BackPlane];

        //if (statePatrol.ParentPlane == ServerRConfig.IsBackPlane[false])
        //    statePatrol.Position.X -= 3; //Enemies on front plane need to be slightly adjusted on X axis due to random bug.

        var nextState = new GameObjectComponents() {
            {"AIStatePatrol", new ComponentSettings()
                {Position.X.ToString(),
                 Position.Y.ToString(),
                 backPlaneZValue.ToString()}
            }
        };

        GoToNextState(nextState);
    }

    public void RunDragonAttackState(object _)
    {
        if (IsAttacking || Id != IdOfAttackingEnemy.ToString())
            return;

        IsAttacking = true;
        var distancesFromEnemy = new Dictionary<Player, double>();

        var closestPlayerPos = GetClosestTarget().TempData.Position;

        var nextState = new GameObjectComponents()
            {
                {"AIStateDrakeAttack", new ComponentSettings()
                {Position.X.ToString(), Position.Y.ToString(), closestPlayerPos.Z.ToString(),
                 Position.X.ToString(), Position.Y.ToString(), closestPlayerPos.Z.ToString(),
                 closestPlayerPos.X.ToString(), closestPlayerPos.Y.ToString(), closestPlayerPos.Z.ToString()}}
            };

        var stateChange = Room.GetEntitiesFromId<AIStatePatrolComp>(IdOfAttackingEnemy).First();

        stateChange.GoToNextState(nextState);

        //Needs method to determine the proper delay time before changing states.
        TimerThread.DelayCall(stateChange.RunPatrol, null, TimeSpan.FromSeconds(10), TimeSpan.Zero, 1);
    }

    public void RunSpikerAttackState(object _)
    {
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

        //Needs projectile implementation.

        var rand = new System.Random();
        var projectileId = Math.Abs(rand.Next()).ToString();

        while (Room.GameObjectIds.Contains(projectileId))
            projectileId = Math.Abs(rand.Next()).ToString();

        // Magic numbers here are temporary

        var direction = closestPlayer.TempData.Position.X < Position.X ? 5 : -5;

        Room.SendSyncEvent(AISyncEventHelper.AILaunchItem(Room.GetEntityFromId<AIStatePatrolComp>(Id),
            Position.X, Position.Y, Position.Z, direction, 0, 3, int.Parse(projectileId), 0));

        var aiProjectile = new AIProjectileEntity(Room, Id, projectileId, Position, 5, 1, 3, TimerThread);
        Room.Projectiles.Add(projectileId, aiProjectile);

        GoToNextState(nextState);

        TimerThread.DelayCall(RunPatrol, null, TimeSpan.FromSeconds(2), TimeSpan.Zero, 1);
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
}
