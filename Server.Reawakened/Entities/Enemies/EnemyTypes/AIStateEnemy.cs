using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using Server.Reawakened.Entities.Enemies.Models;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;

namespace Server.Reawakened.Entities.Enemies.EnemyTypes;

public class AIStateEnemy(EnemyData data) : BaseEnemy(data)
{
    public override void Initialize()
    {
        Room.SendSyncEvent(
            GetBlankEnemyInit(
                Position.x, Position.y, Position.z,
                Position.x, Position.y
            )
        );

        base.Initialize();
    }

    public override void SendAiData(Player player) =>
        Room.SendSyncEvent(
            GetBlankEnemyInit(
                Position.x, Position.y, Position.z,
                Position.x, Position.y
            )
        );
}
