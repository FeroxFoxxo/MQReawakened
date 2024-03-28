using Server.Reawakened.Entities.AIBehavior;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;

namespace Server.Reawakened.Entities.Enemies.EnemyAI.BehaviorEnemies;
public class EnemyOrchid(Room room, string entityId, string prefabName, EnemyControllerComp enemyController, IServiceProvider services) : BehaviorEnemy(room, entityId, prefabName, enemyController, services)
{

    private float _behaviorEndTime;
    private string _offensiveBehavior;

    public override void Initialize()
    {
        base.Initialize();

        MinBehaviorTime = Convert.ToSingle(BehaviorList.GetGlobalProperty("MinBehaviorTime"));
        _offensiveBehavior = Convert.ToString(BehaviorList.GetGlobalProperty("OffensiveBehavior"));
        EnemyGlobalProps.Global_DetectionLimitedByPatrolLine = Convert.ToBoolean(BehaviorList.GetGlobalProperty("DetectionLimitedByPatrolLine"));
        EnemyGlobalProps.Global_FrontDetectionRangeX = Convert.ToSingle(BehaviorList.GetGlobalProperty("FrontDetectionRangeX"));
        EnemyGlobalProps.Global_FrontDetectionRangeUpY = Convert.ToSingle(BehaviorList.GetGlobalProperty("FrontDetectionRangeUpY"));
        EnemyGlobalProps.Global_FrontDetectionRangeDownY = Convert.ToSingle(BehaviorList.GetGlobalProperty("FrontDetectionRangeDownY"));
        EnemyGlobalProps.Global_BackDetectionRangeX = Convert.ToSingle(BehaviorList.GetGlobalProperty("BackDetectionRangeX"));
        EnemyGlobalProps.Global_ShootOffsetX = Convert.ToSingle(BehaviorList.GetGlobalProperty("ShootOffsetX"));
        EnemyGlobalProps.Global_ShootOffsetY = Convert.ToSingle(BehaviorList.GetGlobalProperty("ShootOffsetY"));
        EnemyGlobalProps.Global_ShootingProjectilePrefabName = BehaviorList.GetGlobalProperty("ProjectilePrefabName").ToString();

        // Address magic numbers when we get to adding enemy effect mods
        Room.SendSyncEvent(AIInit(1, 1, 1));
        Room.SendSyncEvent(Utils.AISyncEventHelper.AIDo(Id, Room.Time, Position, 1.0f, BehaviorList.IndexOf("Patrol"), string.Empty, Position.x, Position.y, 1, false));

        // Set these calls to the xml later. Instead of using hardcoded "Patrol", "Aggro", etc.
        // the XML can just specify which behaviors to use when attacked, when moving, etc.
        AiBehavior = ChangeBehavior("Patrol");
    }

    public override void Damage(int damage, Player player)
    {
        base.Damage(damage, player);

        if (AiBehavior is not AIBehaviorShooting)
        {
            Room.SendSyncEvent(Utils.AISyncEventHelper.AIDo(Id, Room.Time, Position, 1.0f, BehaviorList.IndexOf(_offensiveBehavior), string.Empty, player.TempData.Position.X,
                    player.TempData.Position.Y, Generic.Patrol_ForceDirectionX, false));

            // For some reason, the SyncEvent doesn't initialize these properly, so I just do them here
            AiData.Sync_TargetPosX = player.TempData.Position.X;
            AiData.Sync_TargetPosY = player.TempData.Position.Y;

            AiBehavior = ChangeBehavior(_offensiveBehavior);

            _behaviorEndTime = ResetBehaviorTime(MinBehaviorTime);
        }
    }

    public override void HandlePatrol()
    {
        base.HandlePatrol();

        DetectPlayers("Shooting");
    }

    public override void HandleShooting()
    {
        base.HandleShooting();

        if (!AiBehavior.Update(ref AiData, Room.Time))
        {
            Room.SendSyncEvent(Utils.AISyncEventHelper.AIDo(Id, Room.Time, Position, 1.0f, BehaviorList.IndexOf("LookAround"), string.Empty, AiData.Sync_TargetPosX, AiData.Sync_TargetPosY,
            AiData.Intern_Dir, false));

            AiBehavior = ChangeBehavior("LookAround");
            _behaviorEndTime = ResetBehaviorTime(Convert.ToSingle(BehaviorList.GetBehaviorStat("LookAround", "lookTime")));
        }
    }

    public override void HandleLookAround()
    {
        base.HandleLookAround();

        DetectPlayers("Shooting");

        if (Room.Time >= _behaviorEndTime)
        {
            Room.SendSyncEvent(Utils.AISyncEventHelper.AIDo(Id, Room.Time, Position, 1.0f, BehaviorList.IndexOf("Patrol"), string.Empty, Position.x, Position.y, AiData.Intern_Dir, false));

            AiBehavior = ChangeBehavior("Patrol");
        }
    }

    public override void DetectPlayers(string behaviorToRun)
    {
        foreach (var player in Room.Players.Values)
            if (PlayerInRange(player.TempData.Position, EnemyGlobalProps.Global_DetectionLimitedByPatrolLine))
            {
                Room.SendSyncEvent(Utils.AISyncEventHelper.AIDo(Id, Room.Time, Position, 1.0f, BehaviorList.IndexOf(behaviorToRun), string.Empty, player.TempData.Position.X,
                    player.TempData.Position.Y, Generic.Patrol_ForceDirectionX, false));

                // For some reason, the SyncEvent doesn't initialize these properly, so I just do them here
                AiData.Sync_TargetPosX = player.TempData.Position.X;
                AiData.Sync_TargetPosY = player.TempData.Position.Y;

                AiBehavior = ChangeBehavior(behaviorToRun);

                _behaviorEndTime = ResetBehaviorTime(MinBehaviorTime);
            }
    }
}
