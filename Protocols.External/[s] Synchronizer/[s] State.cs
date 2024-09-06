using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Logging;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Projectiles;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Timers;
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
    public ServerRConfig ServerRConfig { get; set; }
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

        if (ServerRConfig.LogSyncState)
            Logger.LogDebug("Found state: {State}", syncEvent.Type);

        var entityId = syncEvent.TargetID;

        var newPlayer = Player.Room.GetPlayerById(entityId);

        if (newPlayer != null)
        {
            switch (syncEvent.Type)
            {
                case SyncEvent.EventType.PetState:
                    if (Player.Character.Pets.TryGetValue(Player.GetEquippedPetId(ServerRConfig), out var pet) &&
                        PetAbilities.PetAbilityData.TryGetValue(int.Parse(pet.PetId), out var petAbilityParams))
                    {
                        Player.Room.SendSyncEvent(new PetState_SyncEvent(Player.GameObjectId, Player.Room.Time, PetInformation.StateSyncType.PetStateVanish, Player.GameObjectId));
                        pet.DespawnPet(Player, petAbilityParams, WorldStatistics, ServerRConfig);
                    }
                    break;
                case SyncEvent.EventType.ChargeAttack:
                    Player.TempData.IsSuperStomping = true;
                    Player.TempData.Invincible = true;

                    var attack = new ChargeAttack_SyncEvent(syncEvent);
                    var superStompDamage = (int)Math.Ceiling(WorldStatistics.GetValue(ItemEffectType.AbilityPower, WorldStatisticsGroup.Player, Player.Character.GlobalLevel) +
                        WorldStatistics.GlobalStats[Globals.StompDamageBonus]) * 2;

                    // Needed because early 2012's ChargeAttack_SyncEvent is different
                    // without it this causes a vs error
                    // not fixable in reawakened it would require using the 2012 codebase
                    if (ServerRConfig.GameVersion <= GameVersion.vPets2012)
                        return;

                    Logger.LogTrace("Super attack is charging: '{Charging}' at ({X}, {Y}) in time: {Delay} " +
                        "at speed ({X}, {Y}) with max pos ({X}, {Y}) for item id: '{Id}' and zone: {Zone}",
                        attack.IsCharging, attack.PosX, attack.PosY, attack.StartDelay,
                        attack.SpeedX, attack.SpeedY, attack.MaxPosX, attack.MaxPosY, attack.ItemId, attack.ZoneId);

                    var chargeAttackProjectile = new ChargeAttackProjectile(
                        Player.GameObjectId, Player,
                        new Vector3() { x = attack.PosX, y = attack.PosY, z = Player.TempData.Position.z },
                        new Vector3() { x = attack.MaxPosX, y = attack.MaxPosY, z = Player.TempData.Position.z },
                        new Vector2() { x = attack.SpeedX, y = attack.SpeedY },
                        15, attack.ItemId, attack.ZoneId, superStompDamage,
                        Elemental.Standard, ServerRConfig, TimerThread
                    );

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

                    newPlayer.TempData.Position = new Vector3
                    {
                        x = physicsBasicEvent.PositionX,
                        y = physicsBasicEvent.PositionY,
                        z = physicsBasicEvent.PositionZ
                    };

                    newPlayer.TempData.Velocity = new Vector3
                    {
                        x = physicsBasicEvent.VelocityX,
                        y = physicsBasicEvent.VelocityY,
                        z = physicsBasicEvent.VelocityZ
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
                case SyncEvent.EventType.PhysicStatus:
                    var physicStatus = new PhysicStatus_SyncEvent(syncEvent);

                    if (physicStatus.GravityEnabled && Player.TempData.Underwater)
                    {
                        Player.StopUnderwater();
                        Player.TempData.Underwater = false;
                    }
                    break;
                case SyncEvent.EventType.FX:
                    var fxEvent = new FX_SyncEvent(syncEvent);

                    if (fxEvent.PrefabName == ServerRConfig.FXWaterSplashName)
                        Player.StartUnderwater(newPlayer.Character.MaxLife / ServerRConfig.UnderwaterDamageRatio, TimerThread, ServerRConfig);
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

    private static void UpdatePlayerCollider(Player player)
    {
        var playerCollider = new PlayerCollider(player);
        playerCollider.IsColliding(false);
        player.Room.OverwriteCollider(playerCollider);
    }

    private void RequestRespawn(string entityId, float triggerTime)
    {
        Player.SendSyncEventToPlayer(new RequestRespawn_SyncEvent(entityId.ToString(), triggerTime));

        Player.TempData.Invincible = true;
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

        TimerThread.RunDelayed(DisableInvincibility, new PlayerTimer() { Player = Player }, TimeSpan.FromSeconds(1.5));
    }

    private static void DisableInvincibility(ITimerData data)
    {
        if (data is not PlayerTimer playerTimer)
            return;

        if (playerTimer.Player.TempData.Invincible)
            playerTimer.Player.TempData.Invincible = false;
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
