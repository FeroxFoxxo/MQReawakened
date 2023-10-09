using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Rooms;
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
    public ServerRConfig ServerConfig { get; set; }
    public FileLogger FileLogger { get; set; }

    public override void Run(string[] message)
    {
        if (message.Length != 7)
        {
            FileLogger.WriteGenericLog<SyncEvent>("sync-errors", "Unknown Protocol", string.Join('\n', message),
                LoggerType.Warning);

            return;
        }

        var room = Player.Room;

        if (room.Entities == null)
            return;

        var syncedData = message[5].Split('&');
        var syncEvent = SyncEventManager.DecodeEvent(syncedData);

        if (ServerConfig.LogSyncState)
            Logger.LogDebug("Found state: {State}", syncEvent.Type);

        var entityId = int.Parse(syncEvent.TargetID);

        if (room.Players.TryGetValue(entityId, out var newPlayer))
        {
            switch (syncEvent.Type)
            {
                case SyncEvent.EventType.ChargeAttack:
                    var chargeAttackEvent = new ChargeAttack_SyncEvent(syncEvent);

                    var startEvent = new ChargeAttackStart_SyncEvent(entityId.ToString(), chargeAttackEvent.TriggerTime,
                        chargeAttackEvent.PosX, chargeAttackEvent.PosY, chargeAttackEvent.SpeedX,
                        chargeAttackEvent.SpeedY,
                        chargeAttackEvent.ItemId, chargeAttackEvent.ZoneId);

                    room.SendSyncEvent(startEvent);

                    Logger.LogWarning("Collision system not yet written and implemented for {Type}.",
                        chargeAttackEvent.Type);
                    break;
                case SyncEvent.EventType.NotifyCollision:
                    var notifyCollisionEvent = new NotifyCollision_SyncEvent(syncEvent);
                    var collisionTarget = int.Parse(notifyCollisionEvent.CollisionTarget);

                    if (room.Entities.TryGetValue(collisionTarget, out var entities))
                        foreach (var entity in entities)
                            entity.NotifyCollision(notifyCollisionEvent, newPlayer);
                    else
                        Logger.LogWarning("Unhandled collision from {TargetId}, no entity for {EntityType}.",
                            collisionTarget, room.GetUnknownEntityTypes(collisionTarget));
                    break;
                case SyncEvent.EventType.PhysicBasic:
                    var physicsBasicEvent = new PhysicBasic_SyncEvent(syncEvent);

                    newPlayer.Position = new Vector3Model
                    {
                        X = physicsBasicEvent.PositionX,
                        Y = physicsBasicEvent.PositionY,
                        Z = physicsBasicEvent.PositionZ
                    };

                    newPlayer.Velocity = new Vector3Model
                    {
                        X = physicsBasicEvent.VelocityX,
                        Y = physicsBasicEvent.VelocityY,
                        Z = physicsBasicEvent.VelocityZ
                    };

                    newPlayer.OnGround = physicsBasicEvent.OnGround;
                    break;
                case SyncEvent.EventType.Direction:
                    var directionEvent = new Direction_SyncEvent(syncEvent);
                    newPlayer.Direction = directionEvent.Direction;
                    break;
            }

            room.SendSyncEvent(syncEvent, Player);
        }
        else if (room.Entities.TryGetValue(entityId, out var entity))
        {
            foreach (var entityComponent in entity)
                entityComponent.RunSyncedEvent(syncEvent, Player);
        }
        else
        {
            switch (syncEvent.Type)
            {
                default:
                    TraceSyncEventError(entityId, syncEvent, room.LevelInfo,
                        room.GetUnknownEntityTypes(entityId));
                    break;
            }
        }

        if (entityId != Player.GameObjectId)
            if (ServerConfig.LogAllSyncEvents)
                LogEvent(syncEvent, entityId, room);
    }

    public void LogEvent(SyncEvent syncEvent, int entityId, Room room)
    {
        var uniqueType = "Unknown";
        var uniqueIdentifier = entityId.ToString();
        var additionalInfo = string.Empty;

        if(room.Players.TryGetValue(entityId, out var newPlayer))
        {
            uniqueType = "Player";

            uniqueIdentifier = newPlayer.Character != null ?
                $"{newPlayer.Character.Data.CharacterName} ({newPlayer.Character.Data.CharacterId})" :
                "Unknown";
        }

        var entities = new List<string>();

        var prefabName = string.Empty;

        if (room.Entities.TryGetValue(entityId, out var entity))
        {
            foreach (var entityComponent in entity)
            {
                entities.Add($"K:{entityComponent.Name}");

                if (!string.IsNullOrEmpty(entityComponent.PrefabName))
                    prefabName = entityComponent.PrefabName;
            }
        }
        
        if (room.UnknownEntities.TryGetValue(entityId, out var unknownEntities))
            foreach (var entityComponent in unknownEntities)
                entities.Add($"U:{entityComponent}");

        if (entities.Count > 0)
        {
            uniqueType = "Entity";

            if (!string.IsNullOrEmpty(prefabName))
                uniqueIdentifier = $"{prefabName} ({entityId})";

            additionalInfo = string.Join('/', entities);
        }

        if (Player.Character != null)
                Logger.LogDebug("SyncEvent '{Type}' run for {Type} [{Id}] by {Player} {AdditionalInfo}",
                    syncEvent.Type, uniqueType, uniqueIdentifier, Player.Character.Data.CharacterName, additionalInfo);
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
