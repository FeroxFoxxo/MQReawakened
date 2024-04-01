using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.EnemyTypes;
public class EnemyBird(Room room, string entityId, string prefabName, EnemyControllerComp enemyController, IServiceProvider services) : BehaviorEnemy(room, entityId, prefabName, enemyController, services)
{
    public override void Initialize()
    {
        // Add enemy props before initialization of base
        EnemyGlobalProps.Global_DetectionLimitedByPatrolLine = Convert.ToBoolean(BehaviorList.GetGlobalProperty("DetectionLimitedByPatrolLine"));
        EnemyGlobalProps.Global_FrontDetectionRangeX = Convert.ToSingle(BehaviorList.GetGlobalProperty("FrontDetectionRangeX"));
        EnemyGlobalProps.Global_FrontDetectionRangeUpY = Convert.ToSingle(BehaviorList.GetGlobalProperty("FrontDetectionRangeUpY"));
        EnemyGlobalProps.Global_FrontDetectionRangeDownY = Convert.ToSingle(BehaviorList.GetGlobalProperty("FrontDetectionRangeDownY"));
        EnemyGlobalProps.Global_BackDetectionRangeX = Convert.ToSingle(BehaviorList.GetGlobalProperty("BackDetectionRangeX"));
        EnemyGlobalProps.Global_ShootOffsetX = Convert.ToSingle(BehaviorList.GetGlobalProperty("ShootOffsetX"));
        EnemyGlobalProps.Global_ShootOffsetY = Convert.ToSingle(BehaviorList.GetGlobalProperty("ShootOffsetY"));
        EnemyGlobalProps.Global_ShootingProjectilePrefabName = BehaviorList.GetGlobalProperty("ProjectilePrefabName").ToString();

        base.Initialize();

        // Address magic numbers when we get to adding enemy effect mods
        Room.SendSyncEvent(AIInit(1, 1, 1));
        Room.SendSyncEvent(AISyncEventHelper.AIDo(Id, Room.Time, Position, 1.0f, BehaviorList.IndexOf(StateTypes.Patrol), string.Empty, Position.x, Position.y, -1, false));

        // Set these calls to the xml later. Instead of using hardcoded "Patrol", "Aggro", etc.
        // the XML can just specify which behaviors to use when attacked, when moving, etc.
        AiBehavior = ChangeBehavior(StateTypes.Patrol);
    }

    public override void Damage(int damage, Player player)
    {
        base.Damage(damage, player);

        if (AiBehavior is not AIBehaviorShooting)
        {
            Room.SendSyncEvent(AISyncEventHelper.AIDo(Id, Room.Time, Position, 1.0f, BehaviorList.IndexOf(OffensiveBehavior), string.Empty, player.TempData.Position.X,
                    player.TempData.Position.Y, Generic.Patrol_ForceDirectionX, false));

            // For some reason, the SyncEvent doesn't initialize these properly, so I just do them here
            AiData.Sync_TargetPosX = player.TempData.Position.X;
            AiData.Sync_TargetPosY = player.TempData.Position.Y;

            AiBehavior = ChangeBehavior(OffensiveBehavior);

            BehaviorEndTime = ResetBehaviorTime(MinBehaviorTime);
        }
    }

    public override void HandlePatrol()
    {
        base.HandlePatrol();

        DetectPlayers(StateTypes.Shooting);
    }

    public override void HandleShooting()
    {
        base.HandleShooting();

        if (!AiBehavior.Update(ref AiData, Room.Time))
        {
            Room.SendSyncEvent(AISyncEventHelper.AIDo(Id, Room.Time, Position, 1.0f, BehaviorList.IndexOf(StateTypes.LookAround), string.Empty, AiData.Sync_TargetPosX, AiData.Sync_TargetPosY,
            AiData.Intern_Dir, false));

            AiBehavior = ChangeBehavior(StateTypes.LookAround);

            var behavior = BehaviorList.BehaviorData[StateTypes.LookAround] as LookAroundState;
            BehaviorEndTime = ResetBehaviorTime(behavior.LookTime);
        }
    }

    public override void HandleLookAround()
    {
        base.HandleLookAround();

        DetectPlayers(StateTypes.Shooting);

        if (Room.Time >= BehaviorEndTime)
        {
            AiBehavior = ChangeBehavior(StateTypes.Patrol);
            Room.SendSyncEvent(AISyncEventHelper.AIDo(Id, Room.Time, Position, 1.0f, BehaviorList.IndexOf(StateTypes.Patrol), string.Empty, Position.x, Position.y, Generic.Patrol_ForceDirectionX, false));
        }
    }

    public override void DetectPlayers(StateTypes type)
    {
        foreach (var player in Room.Players.Values)
            if (PlayerInRange(player.TempData.Position, EnemyGlobalProps.Global_DetectionLimitedByPatrolLine))
            {
                Room.SendSyncEvent(AISyncEventHelper.AIDo(Id, Room.Time, Position, 1.0f, BehaviorList.IndexOf(type), string.Empty, player.TempData.Position.X,
                    player.TempData.Position.Y, Generic.Patrol_ForceDirectionX, false));

                // For some reason, the SyncEvent doesn't initialize these properly, so I just do them here
                AiData.Sync_TargetPosX = player.TempData.Position.X;
                AiData.Sync_TargetPosY = player.TempData.Position.Y;

                AiBehavior = ChangeBehavior(type);

                BehaviorEndTime = ResetBehaviorTime(MinBehaviorTime);
            }
    }
}
