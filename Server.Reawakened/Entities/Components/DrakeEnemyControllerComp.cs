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
    public ServerRConfig ServerRConfig { get; set; }
    public TimerThread TimerThread { get; set; }
    public ILogger<SpiderBossControllerComp> Logger { get; set; }

    public bool IsAttacking = false;
    public int IdOfAttackingEnemy;
    public int TempHealth = 100;

    public override void Update()
    {
        var isColliding = false;
        var playerDistances = new Dictionary<Player, double>();
        var drake = Room.GetComponentsOfType<DrakeEnemyControllerComp>().Where(d => d.Key == Id).First().Value;

        var closestPlayer = GetClosestTarget();

        var detectionRangeCollider = new EnemyCollider(drake.Id, drake.Position, 8, 4, drake.ParentPlane, Room);
        isColliding = detectionRangeCollider.CheckPlayerCollision(new PlayerCollider(closestPlayer));

        if (isColliding)
        {
            IdOfAttackingEnemy = int.Parse(drake.Id);
            RunDragonAttackState(null);
        }
    }

    public void RunDragonAttackState(object _)
    {
        var drake = Room.GetComponentsOfType<DrakeEnemyControllerComp>().Where(d => d.Key == Id).First().Value;

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

        var stateChange = Room.Entities[IdOfAttackingEnemy.ToString()].FirstOrDefault(x => x is AIStatePatrolComp) as AIStatePatrolComp;

        stateChange.GoToNextState(nextState);
        //Needs method to determine the proper delay time before changing states.
        TimerThread.DelayCall(stateChange.RunPatrol, null, TimeSpan.FromSeconds(10), TimeSpan.Zero, 1);
    }

    public Player GetClosestTarget()
    {
        var distancesFromEnemy = new Dictionary<Player, double>();
        foreach (var player in Room.Players.Values)
        {
            var playerParentPlane = player.TempData.Position.Z == 0 ? ServerRConfig.IsBackPlane[false] : ServerRConfig.IsBackPlane[true];
            if (ParentPlane != playerParentPlane)
                return null;

            var playerPos = player.TempData.Position;
            var enemyPos = Position;

            var closestDistance = Math.Sqrt(Math.Pow(enemyPos.X - playerPos.X, 2) + Math.Pow(enemyPos.Y - playerPos.Y, 2));

            distancesFromEnemy.Add(player, closestDistance);
        }

        return distancesFromEnemy.Keys.Min();
    }
}
