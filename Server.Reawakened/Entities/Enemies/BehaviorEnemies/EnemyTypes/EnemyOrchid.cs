using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Rooms;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.EnemyTypes;
public class EnemyOrchid(EnemyData data) : BehaviorEnemy(data)
{
    // Uses Intern_Dir instead of Patrol_ForceDirectionX
    public override void HandleLookAround()
    {
        DetectPlayers(GenericScript.AttackBehavior);

        if (Room.Time >= BehaviorEndTime)
            ChangeBehavior(GenericScript.UnawareBehavior, Position.x, Position.y, AiData.Intern_Dir);
    }
}
