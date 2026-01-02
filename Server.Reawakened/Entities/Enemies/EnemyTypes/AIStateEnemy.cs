using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using Server.Reawakened.Entities.Enemies.Extensions;
using Server.Reawakened.Entities.Enemies.Models;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.EnemyTypes;

public class AIStateEnemy(EnemyData data) : BaseEnemy(data)
{
    public override void Initialize()
    {
        var stateMachine = Room.GetEntityFromId<IAIStateMachine>(Id);

        if (stateMachine == null)
        {
            Logger.LogError("Enemy for: '{Name}' does not have a state machine! " +
                "Are you sure it is an AI state enemy?", PrefabName);
        }
        else
            stateMachine.SetAIStateEnemy(this);

        base.Initialize();
        
        Room.SendSyncEvent(
            GetBlankEnemyInit(
                Position.X, Position.Y, Position.Z,
                Position.X, Position.Y
            )
        );

        Room.SendSyncEvent(
            AISyncEventHelper.AIDo(
                Id, Room,
                Position.X, Position.Y, 1.0f,
                Position.X, Position.Y, 1,
                false, AISyncEventHelper.IndexOf(StateType.Unknown, []), string.Empty
            )
        );
    }

    public override void SendAiData(Player player, bool sendAIDo)
    {
        player.SendSyncEventToPlayer(
            GetBlankEnemyInit(
                Position.X, Position.Y, Position.Z,
                Position.X, Position.Y
            )
        );

        if (sendAIDo)
            player.SendSyncEventToPlayer(
                AISyncEventHelper.AIDo(
                    Id, Room,
                    Position.X, Position.Y, 1.0f,
                    Position.X, Position.Y, 1,
                    false, AISyncEventHelper.IndexOf(StateType.Unknown, []), string.Empty
                )
            );
    }

    public AIInit_SyncEvent GetBlankEnemyInit(float posX, float posY, float posZ, float spawnX, float spawnY) =>
        AISyncEventHelper.AIInit(
            Id, Room,
            posX, posY, posZ, spawnX, spawnY, 0,
            Health, MaxHealth, HealthModifier, ScaleModifier, ResistanceModifier,
            Status.Stars, Level, AISyncEventHelper.CreateDefaultGlobalProperties(), []
        );
}
