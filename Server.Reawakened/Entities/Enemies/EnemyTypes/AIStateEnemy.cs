using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using Server.Reawakened.Entities.Enemies.Extensions;
using Server.Reawakened.Entities.Enemies.Models;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities; // For BaseComponent

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
                Position.x, Position.y, Position.z,
                Position.x, Position.y
            )
        );
    }

    protected override bool TryGetAuthoritativePosition(out float x, out float y, out float z)
    {
        var machine = Room.GetEntityFromId<IAIStateMachine>(Id);

        if (machine is BaseComponent comp)
        {
            x = comp.Position.X;
            y = comp.Position.Y;
            z = comp.Position.Z;
            return true;
        }

        x = Position.x; y = Position.y; z = Position.z;

        return true;
    }

    public override void SendAiData(Player player) =>
        Room.SendSyncEvent(
            GetBlankEnemyInit(
                Position.x, Position.y, Position.z,
                Position.x, Position.y
            )
        );

    public AIInit_SyncEvent GetBlankEnemyInit(float posX, float posY, float posZ, float spawnX, float spawnY) =>
        AISyncEventHelper.AIInit(
            Id, Room,
            posX, posY, posZ, spawnX, spawnY, 0,
            Health, MaxHealth, HealthModifier, ScaleModifier, ResistanceModifier,
            Status.Stars, Level, AISyncEventHelper.CreateDefaultGlobalProperties(), []
        );
}
