using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using Server.Reawakened.Entities.Enemies.Models;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;

namespace Server.Reawakened.Entities.Enemies.EnemyTypes;

public class AIStateEnemy(EnemyData data) : BaseEnemy(data)
{
    public override void Initialize()
    {
        var stateMachine = Room.GetEntityFromId<IAIStateMachine>(Id);

        if (stateMachine == null)
        {
            Logger.LogError("Enemy for '{Name}' does not have a state machine! " +
                "Are you sure it is an AI state enemy?", PrefabName);
            return;
        }

        stateMachine.SetAIStateEnemy(this);

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
