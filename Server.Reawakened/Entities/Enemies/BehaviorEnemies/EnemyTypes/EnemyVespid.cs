using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Models.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.EnemyTypes;
public class EnemyVespid(Room room, string entityId, string prefabName, EnemyControllerComp enemyController, IServiceProvider services) : BehaviorEnemy(room, entityId, prefabName, enemyController, services)
{
    public override void Initialize()
    {
        // Add enemy props before initialization of base
        EnemyGlobalProps.Global_DetectionLimitedByPatrolLine = Convert.ToBoolean(BehaviorList.GetGlobalProperty("DetectionLimitedByPatrolLine"));
        EnemyGlobalProps.Global_FrontDetectionRangeX = Convert.ToSingle(BehaviorList.GetGlobalProperty("FrontDetectionRangeX"));
        EnemyGlobalProps.Global_FrontDetectionRangeUpY = Convert.ToSingle(BehaviorList.GetGlobalProperty("FrontDetectionRangeUpY"));
        EnemyGlobalProps.Global_FrontDetectionRangeDownY = Convert.ToSingle(BehaviorList.GetGlobalProperty("FrontDetectionRangeDownY"));
        EnemyGlobalProps.Global_BackDetectionRangeX = Convert.ToSingle(BehaviorList.GetGlobalProperty("BackDetectionRangeX"));
        EnemyGlobalProps.Global_BackDetectionRangeUpY = Convert.ToSingle(BehaviorList.GetGlobalProperty("BackDetectionRangeUpY"));
        EnemyGlobalProps.Global_BackDetectionRangeDownY = Convert.ToSingle(BehaviorList.GetGlobalProperty("BackDetectionRangeDownY"));

        base.Initialize();

        AiData.Intern_Dir = Generic.Patrol_ForceDirectionX;

        // Address magic numbers when we get to adding enemy effect mods
        Position.z = 10;
        Room.SendSyncEvent(AIInit(1, 1, 1));
        Room.SendSyncEvent(AISyncEventHelper.AIDo(Id, Room.Time, Position, 1.0f, BehaviorList.IndexOf(StateTypes.Patrol), string.Empty, Position.x, Position.y, AiData.Intern_Dir, false));

        // Set these calls to the xml later. Instead of using hardcoded "Patrol", "Aggro", etc.
        // the XML can just specify which behaviors to use when attacked, when moving, etc.
        AiBehavior = ChangeBehavior(StateTypes.Patrol);
    }

    public override void Damage(int damage, Player player)
    {
        base.Damage(damage, player);

        if (AiBehavior is not AIBehaviorShooting)
        {
            var aiEvent = AISyncEventHelper.AIDo(
                Id, Room.Time, Position, 1.0f, BehaviorList.IndexOf(OffensiveBehavior), string.Empty,
                player.TempData.Position.X, player.TempData.Position.Y, Generic.Patrol_ForceDirectionX, false
            );

            Room.SendSyncEvent(aiEvent);

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

        if (Room.Time >= BehaviorEndTime)
            DetectPlayers(StateTypes.Stinger);
    }

    public override void HandleStinger()
    {
        base.HandleStinger();

        if (!AiBehavior.Update(ref AiData, Room.Time))
        {
            Room.SendSyncEvent(AISyncEventHelper.AIDo(Id, Room.Time, Position, 1.0f, BehaviorList.IndexOf(StateTypes.Patrol), string.Empty, Position.x, Position.y, AiData.Intern_Dir, false));

            AiBehavior = ChangeBehavior(StateTypes.Patrol);
        }
    }

    public override void DetectPlayers(StateTypes behaviorToRun)
    {
        foreach (var player in Room.Players)
            if (PlayerInRange(player.Value.TempData.Position, EnemyGlobalProps.Global_DetectionLimitedByPatrolLine))
            {
                Room.SendSyncEvent(AISyncEventHelper.AIDo(Id, Room.Time, Position, 1.0f, BehaviorList.IndexOf(behaviorToRun), string.Empty, player.Value.TempData.Position.X,
                    Position.y, Generic.Patrol_ForceDirectionX, false));

                // For some reason, the SyncEvent doesn't initialize these properly, so I just do them here
                AiData.Sync_TargetPosX = player.Value.TempData.Position.X;
                AiData.Sync_TargetPosY = Position.y;

                AiBehavior = ChangeBehavior(behaviorToRun);

                BehaviorEndTime = ResetBehaviorTime(MinBehaviorTime);
            }
    }
}
