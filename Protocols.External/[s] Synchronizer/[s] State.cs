using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.Rooms.Services;
using System.Text;
using WorldGraphDefines;

namespace Protocols.External._s__Synchronizer;

public class State : ExternalProtocol
{
    public override string ProtocolName => "ss";

    public SyncEventManager SyncEventManager { get; set; }
    public ServerRConfig ServerConfig { get; set; }
    public FileLogger FileLogger { get; set; }
    public TimerThread TimerThread { get; set; }
    public ILogger<State> Logger { get; set; }

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

        var entityId = syncEvent.TargetID;

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
                    var collisionTarget = notifyCollisionEvent.CollisionTarget;

                    if (room.Entities.TryGetValue(collisionTarget, out var entityComponents))
                    {
                        foreach (var component in entityComponents)
                            component.NotifyCollision(notifyCollisionEvent, newPlayer);
                    }
                    else
                        Logger.LogWarning("Unhandled collision from {TargetId}, no entity for {EntityType}.",
                            collisionTarget, room.GetUnknownComponentTypes(collisionTarget));
                    break;
                case SyncEvent.EventType.PhysicBasic:
                    var physicsBasicEvent = new PhysicBasic_SyncEvent(syncEvent);

                    newPlayer.TempData.Position = new Vector3Model
                    {
                        X = physicsBasicEvent.PositionX,
                        Y = physicsBasicEvent.PositionY,
                        Z = physicsBasicEvent.PositionZ
                    };

                    newPlayer.TempData.Velocity = new Vector3Model
                    {
                        X = physicsBasicEvent.VelocityX,
                        Y = physicsBasicEvent.VelocityY,
                        Z = physicsBasicEvent.VelocityZ
                    };

                    newPlayer.TempData.OnGround = physicsBasicEvent.OnGround;
                    break;
                case SyncEvent.EventType.Direction:
                    var directionEvent = new Direction_SyncEvent(syncEvent);
                    newPlayer.TempData.Direction = directionEvent.Direction;
                    break;
                case SyncEvent.EventType.RequestRespawn:
                    RequestRespawn(entityId, syncEvent.TriggerTime);
                    break;
                case SyncEvent.EventType.PhysicStatus:
                    foreach (var entity in room.Entities)
                    {
                        foreach (var comp in entity.Value)
                        {
                            if (comp is HazardControllerComp hazard)
                            {
                                Enum.TryParse(hazard.HurtEffect, true, out ItemEffectType effectType);

                                if (effectType == ItemEffectType.SlowStatusEffect)
                                {
                                    var distanceXThreshold = 4.0f;
                                    var distanceYThreshold = 1f;
                                    var distanceX = Math.Abs(Player.TempData.Position.X - comp.Entity.GameObject.ObjectInfo.Position.X + -4.0f);
                                    var distanceY = Math.Abs(Player.TempData.Position.Y - comp.Entity.GameObject.ObjectInfo.Position.Y + 4);

                                    if (distanceX <= distanceXThreshold && distanceY <= distanceYThreshold)
                                    {
                                        var slowStatusEffect = new StatusEffect_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time,
                                            (int)ItemEffectType.SlowStatusEffect, 1, 1, true, comp.PrefabName, false);

                                        Player.Room.SendSyncEvent(slowStatusEffect);
                                    }
                                }
                            }
                            if (comp is CollapsingPlatformComp platform)
                            {
                                var distanceXThreshold = 1.0f;
                                var distanceYThreshold = 1f;
                                var distanceX = Math.Abs(Player.TempData.Position.X - comp.Entity.GameObject.ObjectInfo.Position.X);
                                var distanceY = Math.Abs(Player.TempData.Position.Y - comp.Entity.GameObject.ObjectInfo.Position.Y + -1.0f);

                                if (distanceX <= distanceXThreshold && distanceY <= distanceYThreshold && !platform.IsBroken)
                                {
                                    platform.Collapse(false);
                                }
                            }
                        }
                    }
                    break;
            }

            room.SendSyncEvent(syncEvent, Player);
        }
        else if (room.Entities.TryGetValue(entityId, out var entityComponents))
        {
            foreach (var component in entityComponents)
                component.RunSyncedEvent(syncEvent, Player);
        }
        else
        {
            switch (syncEvent.Type)
            {
                case SyncEvent.EventType.RequestRespawn:
                    RequestRespawn(Player.GameObjectId, syncEvent.TriggerTime);
                    break;
                default:
                    TraceSyncEventError(entityId, syncEvent, room.LevelInfo,
                        room.GetUnknownComponentTypes(entityId));
                    break;
            }
        }

        if (entityId != Player.GameObjectId)
            if (ServerConfig.LogAllSyncEvents)
                LogEvent(syncEvent, entityId, room);
    }

    private void RequestRespawn(string entityId, float triggerTime)
    {
        Player.Room.SendSyncEvent(new RequestRespawn_SyncEvent(entityId.ToString(), triggerTime));

        Player.TempData.Invincible = true;
        Player.Character.Data.CurrentLife = Player.Character.Data.MaxLife;

        Player.Room.SendSyncEvent(new Health_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time,
            Player.Character.Data.MaxLife, Player.Character.Data.MaxLife, Player.GameObjectId.ToString()));

        BaseComponent respawnPosition = Player.Room.LastCheckpoint != null ? Player.Room.LastCheckpoint : Player.Room.DefaultSpawn;

        Player.Room.SendSyncEvent(new PhysicTeleport_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time,
                 respawnPosition.Position.X, respawnPosition.Position.Y, respawnPosition.IsOnBackPlane(Logger)));

        TimerThread.DelayCall(DisableInvincibility, Player, TimeSpan.FromSeconds(1.5), TimeSpan.Zero, 1);
    }

    private static void DisableInvincibility(object playerObj)
    {
        var player = (Player)playerObj;

        if (player == null)
            return;

        if (player.TempData == null)
            return;

        if (player.TempData.Invincible)
            player.TempData.Invincible = false;
    }

    public void LogEvent(SyncEvent syncEvent, string entityId, Room room)
    {
        var uniqueType = "Unknown";
        var uniqueIdentifier = entityId;
        var additionalInfo = string.Empty;

        if (room.Players.TryGetValue(entityId, out var newPlayer))
        {
            uniqueType = "Player";

            uniqueIdentifier = newPlayer.Character != null ?
                $"{newPlayer.CharacterName} ({newPlayer.CharacterId})" :
                "Unknown";
        }

        var entityComponentList = new List<string>();

        var prefabName = string.Empty;

        if (room.Entities.TryGetValue(entityId, out var entityComponents))
        {
            foreach (var component in entityComponents)
            {
                entityComponentList.Add($"K:{component.Name}");

                if (!string.IsNullOrEmpty(component.PrefabName))
                    prefabName = component.PrefabName;
            }
        }

        if (room.UnknownEntities.TryGetValue(entityId, out var unknownEntityComponents))
            foreach (var component in unknownEntityComponents)
                entityComponentList.Add($"U:{component}");

        if (entityComponentList.Count > 0)
        {
            uniqueType = "Entity";

            if (!string.IsNullOrEmpty(prefabName))
                uniqueIdentifier = $"{prefabName} ({entityId})";

            additionalInfo = string.Join('/', entityComponentList);
        }

        var attributes = string.Join(", ", syncEvent.EventDataList);

        if (Player.Character != null)
            Logger.LogDebug("SyncEvent '{Type}' run for {Type} [{Id}] by {Player} {AdditionalInfo} with attributes {Attrib}",
                syncEvent.Type, uniqueType, uniqueIdentifier, Player.CharacterName, additionalInfo, attributes);
    }

    public void TraceSyncEventError(string entityId, SyncEvent syncEvent, LevelInfo levelInfo, string entityInfo)
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
