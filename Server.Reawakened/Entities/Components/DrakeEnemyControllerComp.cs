using Server.Base.Timers.Services;
using Server.Reawakened.Entities.AIStates;
using static A2m.Server.ExtLevelEditor;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Extensions;
using Server.Base.Timers.Extensions;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Players;
using Server.Reawakened.Configs;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Components;

public class DrakeEnemyControllerComp : Component<DrakeEnemyController>
{
    public TimerThread TimerThread { get; set; }
    public ILogger<DrakeEnemyControllerComp> Logger { get; set; }

    public bool IsAttacking = false;
    public string IdOfAttackingEnemy;
    public int TempHealth = 100;

    public override void Update()
    {
        var drake = Room.GetEntitiesFromId<DrakeEnemyControllerComp>(Id).First();

        var closestPlayer = GetClosestTarget();

        if (closestPlayer == null) return;

        var detectionRangeCollider = new EnemyCollider(drake.Id, drake.Position, 8, 4, drake.ParentPlane, Room);
        var isColliding = detectionRangeCollider.CheckCollision(new PlayerCollider(closestPlayer));

        if (isColliding)
        {
            IdOfAttackingEnemy = drake.Id;
            RunDragonAttackState(null);
        }
    }

    public void RunDragonAttackState(object _)
    {
        var drake = Room.GetEntitiesFromId<DrakeEnemyControllerComp>(Id).First();

        if (IsAttacking || drake.Id != IdOfAttackingEnemy.ToString())
            return;

        IsAttacking = true;
        var distancesFromEnemy = new Dictionary<Player, double>();

        var closestPlayerPos = GetClosestTarget().TempData.Position;

        var nextState = new GameObjectComponents()
            {
                {"AIStateDrakeAttack", new ComponentSettings()
                {drake.Position.X.ToString(), drake.Position.Y.ToString(), closestPlayerPos.Z.ToString(),
                 drake.Position.X.ToString(), drake.Position.Y.ToString(), closestPlayerPos.Z.ToString(),
                 closestPlayerPos.X.ToString(), closestPlayerPos.Y.ToString(), closestPlayerPos.Z.ToString()}}
            };

        var stateChange = Room.GetEntitiesFromType<AIStatePatrolComp>()
            .Where(x => x.Id == IdOfAttackingEnemy).First();

        stateChange.GoToNextState(nextState);
        //Needs method to determine the proper delay time before changing states.
        TimerThread.DelayCall(stateChange.RunPatrol, null, TimeSpan.FromSeconds(10), TimeSpan.Zero, 1);
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
}
