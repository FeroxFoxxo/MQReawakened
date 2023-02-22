using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.Rooms.Services;
using System.Text;
using WorldGraphDefines;

namespace Protocols.External._s__Synchronizer;

public class State : ExternalProtocol
{
    public override string ProtocolName => "ss";

    public SyncEventManager SyncEventManager { get; set; }
    public ILogger<State> Logger { get; set; }
    public ServerStaticConfig ServerConfig { get; set; }
    public FileLogger FileLogger { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();

        if (player.CurrentRoom.Entities == null)
            return;

        var syncEvent = SyncEventManager.DecodeEvent(message[5].Split('&'));

        if (ServerConfig.LogSyncState)
            Logger.LogDebug("Found state: {State}", syncEvent.Type);

        var entityId = int.Parse(syncEvent.TargetID);

        if (entityId == player.GameObjectId)
        {
            switch (syncEvent.Type)
            {
                case SyncEvent.EventType.ChargeAttack:
                    var chargeAttackEvent = new ChargeAttack_SyncEvent(syncEvent);

                    var startEvent = new ChargeAttackStart_SyncEvent(entityId.ToString(), chargeAttackEvent.TriggerTime,
                        chargeAttackEvent.PosX, chargeAttackEvent.PosY, chargeAttackEvent.SpeedX,
                        chargeAttackEvent.SpeedY,
                        chargeAttackEvent.ItemId, chargeAttackEvent.ZoneId);

                    NetState.SendSyncEventToPlayer(startEvent);

                    Logger.LogWarning("Collision system not yet written and implemented for {Type}.",
                        chargeAttackEvent.Type);
                    break;
                case SyncEvent.EventType.NotifyCollision:
                    var notifyCollisionEvent = new NotifyCollision_SyncEvent(syncEvent);
                    var collisionTarget = int.Parse(notifyCollisionEvent.CollisionTarget);

                    if (player.CurrentRoom.Entities.TryGetValue(collisionTarget, out var entities))
                        foreach (var entity in entities)
                            entity.NotifyCollision(notifyCollisionEvent, NetState);
                    else
                        Logger.LogWarning("Unhandled collision from {TargetId}, no entity for {EntityType}.",
                            collisionTarget, player.CurrentRoom.GetUnknownEntityTypes(collisionTarget));
                    break;
                case SyncEvent.EventType.PhysicBasic:
                    var physicsBasicEvent = new PhysicBasic_SyncEvent(syncEvent);

                    player.Position = new Vector3Model
                    {
                        X = physicsBasicEvent.PositionX,
                        Y = physicsBasicEvent.PositionY,
                        Z = physicsBasicEvent.PositionZ
                    };

                    player.Velocity = new Vector3Model
                    {
                        X = physicsBasicEvent.VelocityX,
                        Y = physicsBasicEvent.VelocityY,
                        Z = physicsBasicEvent.VelocityZ
                    };

                    player.OnGround = physicsBasicEvent.OnGround;
                    break;
                case SyncEvent.EventType.Direction:
                    var directionEvent = new Direction_SyncEvent(syncEvent);
                    player.Direction = directionEvent.Direction;
                    break;
            }

            player.CurrentRoom.SendSyncEvent(syncEvent, player);
        }
        else if (player.CurrentRoom.Entities.TryGetValue(entityId, out var entities))
        {
            foreach (var entity in entities)
                entity.RunSyncedEvent(syncEvent, NetState);
        }
        else
        {
            switch (syncEvent.Type)
            {
                default:
                    TraceSyncEventError(entityId, syncEvent, player.CurrentRoom.LevelInfo,
                        player.CurrentRoom.GetUnknownEntityTypes(entityId));
                    break;
            }
        }
    }

    public void TraceSyncEventError(int entityId, SyncEvent syncEvent, LevelInfo levelInfo, string entityInfo)
    {
        var builder = new StringBuilder()
            .AppendLine($"Entity: {entityId}")
            .AppendLine(entityInfo)
            .AppendLine($"Level: {levelInfo.LevelId} ({levelInfo.InGameName})")
            .AppendLine($"Event Type: {syncEvent.Type}")
            .Append($"Data: {syncEvent.EncodeData()}");

        FileLogger.WriteGenericLog<SyncEvent>("event-errors", "State Change", builder.ToString(), LoggerType.Warning);
    }
}
