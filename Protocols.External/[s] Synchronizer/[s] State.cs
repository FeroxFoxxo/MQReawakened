using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.Entities.Projectiles;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Bundles.Base;
using System.Text;
using UnityEngine;
using WorldGraphDefines;

namespace Protocols.External._s__Synchronizer;

public class State : ExternalProtocol
{
    public override string ProtocolName => "ss";
    public PetAbilities PetAbilities { get; set; }
    public SyncEventManager SyncEventManager { get; set; }
    public ItemRConfig ItemRConfig { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public WorldStatistics WorldStatistics { get; set; }
    public FileLogger FileLogger { get; set; }
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

        if (ServerRConfig.LogSyncState)
            Logger.LogDebug("Found state: {State}", syncEvent.Type);

        var entityId = syncEvent.TargetID;

        var newPlayer = Player.Room.GetPlayerById(entityId);

        if (newPlayer != null)
        {
            switch (syncEvent.Type)
            {
                case SyncEvent.EventType.PetState:
                    if (Player.Character.Pets.TryGetValue(Player.Character.PetItemId.ToString(), out var pet) &&
                        PetAbilities.PetAbilityData.TryGetValue(int.Parse(pet.ItemId), out var petAbilityParams))
                    {
                        Player.Room.SendSyncEvent(new PetState_SyncEvent(Player.GameObjectId, Player.Room.Time, PetInformation.StateSyncType.PetStateVanish, Player.GameObjectId));
                        Player.UnequipPet(pet.ItemId, petAbilityParams, ItemCatalog, WorldStatistics, ItemRConfig);
                    }
                    break;
                case SyncEvent.EventType.ChargeAttack:
                    Player.TempData.IsSuperStomping = true;

                    var attack = new ChargeAttack_SyncEvent(syncEvent);

                    var itemId = -1;
                    var zoneId = -1;
                    
                    itemId = attack.ItemId;
                    
                    if (ServerRConfig.GameVersion > GameVersion.vPets2012)
                        zoneId = attack.ZoneId;

                    Logger.LogTrace("Super attack is charging: '{Charging}' at ({X}, {Y}) in time: {Delay} " +
                        "at speed ({X}, {Y}) with max pos ({X}, {Y}) for item id: '{Id}' and zone: {Zone}",
                        attack.IsCharging, attack.PosX, attack.PosY, attack.StartDelay,
                        attack.SpeedX, attack.SpeedY, attack.MaxPosX, attack.MaxPosY, itemId, zoneId);

                    Player.RemovePlayerProjectile();

                    Player.TempData.ProjectileId = Player.Room.CreateProjectileId();

                    var chargeAttackProjectile = new ChargeAttackProjectile(
                        Player.TempData.ProjectileId.ToString(), Player,
                        new Vector3Model(attack.PosX, attack.PosY, Player.TempData.Position.Z),
                        new Vector3(attack.MaxPosX, attack.MaxPosY, Player.TempData.Position.Z),
                        new Vector2(attack.SpeedX, attack.SpeedY),
                        15, ItemCatalog.GetItemFromId(itemId), zoneId,
                        ServerRConfig, ItemCatalog
                    );

                    Player.Room.AddProjectile(chargeAttackProjectile);
                    break;
                case SyncEvent.EventType.ChargeAttackStop:
                    Player.TempData.IsSuperStomping = false;

                    Player.RemovePlayerProjectile();
                    
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

                    if (!physicsBasicEvent.OnGround && newPlayer.TempData.IsSlowed)
                        newPlayer.NullifySlowStatusEffect();
                    
                    newPlayer.TempData.Position.SetPosition(
                        physicsBasicEvent.PositionX,
                        physicsBasicEvent.PositionY,
                        physicsBasicEvent.PositionZ
                    );

                    newPlayer.TempData.Velocity = new Vector3
                    {
                        x = physicsBasicEvent.VelocityX,
                        y = physicsBasicEvent.VelocityY,
                        z = physicsBasicEvent.VelocityZ
                    };

                    newPlayer.TempData.OnGround = physicsBasicEvent.OnGround;

                    if (newPlayer.TempData.Position.Z is not 0 and not 20)
                        newPlayer.TemporaryInvincibility(ServerRConfig.CrossBridgeTime);

                    UpdatePlayerCollider(newPlayer);
                    break;
                case SyncEvent.EventType.Direction:
                    var directionEvent = new Direction_SyncEvent(syncEvent);
                    newPlayer.TempData.Direction = directionEvent.Direction;
                    break;
                case SyncEvent.EventType.RequestRespawn:
                    RequestRespawn(entityId, syncEvent.TriggerTime);
                    break;
                case SyncEvent.EventType.PhysicStatus:
                    var physicStatus = new PhysicStatus_SyncEvent(syncEvent);

                    if (physicStatus.GravityEnabled && Player.TempData.Underwater)
                    {
                        Player.ResetUnderwaterTime();
                        Player.TempData.Underwater = false;
                    }
                    else if (Player.TempData.Underwater)
                    {
                        Player.RunUnderwaterTick(ServerRConfig);
                    }
                    else if (Player.TempData.Underwater && Player.TempData.UnderwaterTime != 0)
                    {
                        Player.ResetUnderwaterTime();
                    }
                    break;
                case SyncEvent.EventType.FX:
                    var fxEvent = new FX_SyncEvent(syncEvent);

                    if (fxEvent.PrefabName == ServerRConfig.FXWaterSplashName)
                        Player.RunUnderwaterTick(ServerRConfig);
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
            if (ServerRConfig.LogAllSyncEvents)
                LogEvent(syncEvent, entityId, Player.Room);
    }

    private static void UpdatePlayerCollider(Player player) => player.TempData.PlayerCollider.RunCollisionDetection();

    private void RequestRespawn(string entityId, float triggerTime)
    {
        Player.TemporaryInvincibility(2);
        Player.SendSyncEventToPlayer(new RequestRespawn_SyncEvent(entityId.ToString(), triggerTime));

        Player.Character.Write.CurrentLife = Player.Character.MaxLife;

        Player.SendSyncEventToPlayer(new Health_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time,
            Player.Character.MaxLife, Player.Character.MaxLife, Player.GameObjectId.ToString()));

        var respawnPosition = Player.Room.LastCheckpoint ?? Player.Room.GetDefaultSpawnPoint();

        if (Player.TempData.CurrentArena is not null)
            Player.SendSyncEventToPlayer(new PhysicTeleport_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time,
            Player.TempData.CurrentArena.Position.X, Player.TempData.CurrentArena.Position.Y, Player.TempData.CurrentArena.IsOnBackPlane(Logger)));
        else
            Player.SendSyncEventToPlayer(new PhysicTeleport_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time,
            respawnPosition.Position.X, respawnPosition.Position.Y, respawnPosition.IsOnBackPlane(Logger)));

        Player.ResetUnderwaterTime();

        Player.TempData.IsKnockedOut = false;
        Player.TempData.Underwater = false;
    }

    public void LogEvent(SyncEvent syncEvent, string entityId, Room room)
    {
        var uniqueType = "unknown";
        var uniqueIdentifier = entityId;
        var additionalInfo = string.Empty;

        var newPlayer = room.GetPlayerById(entityId);

        if (newPlayer != null)
        {
            uniqueType = "player";

            uniqueIdentifier = newPlayer.Character != null ?
                $"'{newPlayer.CharacterName}' ({newPlayer.CharacterId})" :
                "'unknown'";
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
            uniqueType = "entity";

            if (!string.IsNullOrEmpty(prefabName))
                uniqueIdentifier = $"'{prefabName}' ({entityId})";

            additionalInfo = $" {string.Join('/', entityComponentList)}";
        }

        var attributes = string.Join(", ", syncEvent.EventDataList);

        if (Player.Character != null)
            Logger.LogDebug("SyncEvent {Type} run for {Type} {Id} by {Player}{AdditionalInfo} with attributes {Attrib}",
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
