using Server.Reawakened.Entities.AIBehavior;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using UnityEngine;

namespace Server.Reawakened.Entities.Entity.Enemies;
public class EnemyOrchid(Room room, string entityId, BaseComponent baseEntity) : Enemy(room, entityId, baseEntity)
{
    public override void Initialize()
    {
    }

    public override void Update()
    {
        base.Update();

        foreach (var player in Room.Players.ToList())
        {
            var pos = player.Value.TempData.Position;
            if (PlayerInRange(pos))
                Room.SendSyncEvent(AIDo(1.0f, 2, "", Position.x, Position.y, Generic.Patrol_ForceDirectionX, 0));
        }
    }

    public override string WriteBehaviorList()
    {
        var output = "Idle||";
        List<string> behaviorList = [];

        PatrolSpeed = 0;
        EndPathWaitTime = 0;

        //temp values for now. make this based on DetectionRange soon
        DetectionRange = new Rect(Position.x - 6, Position.y, 12, 6);

        behaviorList.Add("Patrol|" + PatrolSpeed + ";" + 0 + ";" + EndPathWaitTime + ";" + Generic.Patrol_DistanceX + ";" + Generic.Patrol_DistanceY + ";" + Generic.Patrol_ForceDirectionX + ";" + Generic.Patrol_InitialProgressRatio + "|");
        behaviorList.Add("Shooting|" + 3 + ";" + 90 + ";" + 0 + ";" + 1 + ";" + 1 + ";" + 2 + ";" + 1 + ";" + 1 + ";" + 4 + ";" + 0 + ";" + 0 + "|" + "PROJ-COL_PRJ_PoisonProjectile_Lv3");
        
        foreach (var bah in behaviorList)
            if (behaviorList.Count > 0)
                output = output + "`" + bah;

        Behavior = new AIBehavior_Patrol(new Vector3(SpawnPosition.x, SpawnPosition.y, SpawnPosition.z),
                   new Vector3(SpawnPosition.x + Generic.Patrol_DistanceX, SpawnPosition.y + Generic.Patrol_DistanceY, SpawnPosition.z),
                   PatrolSpeed,
                   EndPathWaitTime);

        return output;
    }
}
