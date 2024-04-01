using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Projectiles;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles;
using System.Text;
using WorldGraphDefines;

namespace Protocols.External._s__Synchronizer;

public class State : ExternalProtocol
{
    public override string ProtocolName => "ss";

    public SyncEventManager SyncEventManager { get; set; }
    public ServerRConfig ServerConfig { get; set; }
    public ItemRConfig ItemRConfig { get; set; }
    public WorldStatistics WorldStatistics { get; set; }
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

        var syncedData = message[5].Split('&');
        var syncEvent = SyncEventManager.DecodeEvent(syncedData);

        if (ServerConfig.LogSyncState)
            Logger.LogDebug("Found state: {State}", syncEvent.Type);

        var entityId = syncEvent.TargetID;

        if (Player.Room.Players.TryGetValue(entityId, out var newPlayer))
        {
            switch (syncEvent.Type)
            {
                case SyncEvent.EventType.ChargeAttack:
                    Player.TempData.IsSuperStomping = true;
                    Player.TempData.Invincible = true;

                    var attack = new ChargeAttack_SyncEvent(syncEvent);

                    Logger.LogDebug("Super attack is charging: '{Charging}' at ({X}, {Y}) in time: {Delay} " +
                        "at speed ({X}, {Y}) with max pos ({X}, {Y}) for item id: '{Id}' and zone: {Zone}",
                        attack.IsCharging, attack.PosX, attack.PosY, attack.StartDelay,
                        attack.SpeedX, attack.SpeedY, attack.MaxPosX, attack.MaxPosY, attack.ItemId, attack.ZoneId);

                    var chargeAttackProjectile = new ChargeAttackProjectile(Player.GameObjectId, Player,
                                        new Vector3Model() { X = attack.PosX, Y = attack.PosY, Z = Player.TempData.Position.Z },
                                        new Vector3Model() { X = attack.MaxPosX, Y = attack.MaxPosY, Z = Player.TempData.Position.Z },
                                        new Vector2Model() { X = attack.SpeedX, Y = attack.SpeedY },
                                        15, attack.ItemId, attack.ZoneId,
                                        WorldStatistics.GetValue(ItemEffectType.AbilityPower, WorldStatisticsGroup.Player, Player.Character.Data.GlobalLevel),
                                        Elemental.Standard, ServerConfig, TimerThread);

                    Player.Room.AddProjectile(chargeAttackProjectile);
                    break;
                case SyncEvent.EventType.ChargeAttackStop:
                    Player.TempData.IsSuperStomping = false;
                    Player.TempData.Invincible = false;

                    Player.Room.RemoveProjectile(Player.GameObjectId);
                    break;
                case SyncEvent.EventType.NotifyCollision:
                    var notifyCollisionEvent = new NotifyCollision_SyncEvent(syncEvent);
                    var collisionTarget = notifyCollisionEvent.CollisionTarget;

                    if (newPlayer.Room.ContainsEntity(collisionTarget))
                    {
                        foreach (var component in Player.Room.GetEntitiesFromId<BaseComponent>(collisionTarget))
                            if (!Player.Room.IsObjectKilled(component.Id))
                                component.NotifyCollision(notifyCollisionEvent, newPlayer);
                    }
                    else
                        Logger.LogWarning("Unhandled collision from {TargetId}, no entity for {EntityType}.",
                            collisionTarget, newPlayer.Room.GetUnknownComponentTypes(collisionTarget));
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

                    UpdatePlayerCollider(newPlayer);
                    break;
                case SyncEvent.EventType.Direction:
                    var directionEvent = new Direction_SyncEvent(syncEvent);
                    newPlayer.TempData.Direction = directionEvent.Direction;
                    break;
                case SyncEvent.EventType.RequestRespawn:
                    RequestRespawn(entityId, syncEvent.TriggerTime);
                    break;
            }

            Player.Room.SendSyncEvent(syncEvent, Player);
        }
        else if (Player.Room.ContainsEntity(entityId))
        {
            foreach (var component in Player.Room.GetEntitiesFromId<BaseComponent>(entityId))
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
                    TraceSyncEventError(entityId, syncEvent, Player.Room.LevelInfo,
                        Player.Room.GetUnknownComponentTypes(entityId));
                    break;
            }
        }

        if (entityId != Player.GameObjectId)
            if (ServerConfig.LogAllSyncEvents)
                LogEvent(syncEvent, entityId, Player.Room);
    }

    private static void UpdatePlayerCollider(Player player)
    {
        var playerCollider = new PlayerCollider(player);
        playerCollider.IsColliding(false);
        player.Room.Colliders[player.GameObjectId] = playerCollider;
    }

    private void RequestRespawn(string entityId, float triggerTime)
    {
        Player.SendSyncEventToPlayer(new RequestRespawn_SyncEvent(entityId.ToString(), triggerTime));

        Player.TempData.Invincible = true;
        Player.Character.Data.CurrentLife = Player.Character.Data.MaxLife;

        Player.SendSyncEventToPlayer(new Health_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time,
            Player.Character.Data.MaxLife, Player.Character.Data.MaxLife, Player.GameObjectId.ToString()));

        BaseComponent respawnPosition = Player.Room.LastCheckpoint != null ? Player.Room.LastCheckpoint : Player.Room.DefaultSpawn;

        Player.SendSyncEventToPlayer(new PhysicTeleport_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time,
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

        foreach (var component in room.GetEntitiesFromId<BaseComponent>(entityId))
        {
            entityComponentList.Add($"K:{component.Name}");

            if (!string.IsNullOrEmpty(component.PrefabName))
                prefabName = component.PrefabName;
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
