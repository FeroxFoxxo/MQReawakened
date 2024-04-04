using A2m.Server;
using AssetStudio;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Components.GameObjects.Breakables;
using Server.Reawakened.Entities.Components.GameObjects.Breakables.Interfaces;
using Server.Reawakened.Entities.Components.GameObjects.Checkpoints;
using Server.Reawakened.Entities.Components.GameObjects.Controllers;
using Server.Reawakened.Entities.Enemies;
using Server.Reawakened.Entities.Enemies.AIStateEnemies;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Extensions;
using Server.Reawakened.Entities.Enemies.Models;
using Server.Reawakened.Entities.Projectiles;
using Server.Reawakened.Entities.Projectiles.Abstractions;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Rooms.Enums;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Entities.Colliders;
using Server.Reawakened.Rooms.Models.Entities.Colliders.Abstractions;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using Server.Reawakened.XMLs.Models.Enemy.Enums;
using System.Collections.Generic;
using UnityEngine;
using WorldGraphDefines;
using Random = System.Random;
using Timer = Server.Base.Timers.Timer;

namespace Server.Reawakened.Rooms;

public class Room : Timer
{
    private readonly object _roomLock;

    private readonly int _roomId;
    private readonly Level _level;

    private readonly SpawnPointComp _defaultSpawn;

    private readonly Dictionary<string, List<BaseComponent>> _entities;
    private readonly Dictionary<string, BaseProjectile> _projectiles;
    private readonly Dictionary<string, BaseEnemy> _enemies;
    private readonly Dictionary<string, Player> _players;
    private readonly Dictionary<string, BaseCollider> _colliders;

    private readonly HashSet<string> _gameObjectIds;
    private readonly HashSet<string> _killedObjects;

    public ILogger<Room> Logger;

    public Dictionary<string, PlaneModel> Planes;
    public Dictionary<string, List<string>> UnknownEntities;
    public Dictionary<string, List<List<BaseComponent>>> DuplicateEntities;

    private readonly ServerRConfig _config;
    private readonly ItemRConfig _itemConfig;
    private readonly TimerThread _timerThread;
    private readonly IServiceProvider _services;

    public ItemCatalog ItemCatalog;
    public InternalColliders ColliderCatalog;
    public InternalEnemyData InternalEnemyData;

    public CheckpointControllerComp LastCheckpoint { get; set; }

    public LevelInfo LevelInfo => _level.LevelInfo;
    public long TimeOffset { get; set; }
    public float Time => (float)((GetTime.GetCurrentUnixMilliseconds() - TimeOffset) / 1000.0);

    public Room(int roomId, Level level, TimerThread timerThread, IServiceProvider services, ServerRConfig config) :
        base(TimeSpan.Zero, TimeSpan.FromSeconds(1.0 / config.RoomTickRate), 0, timerThread)
    {
        _roomLock = new object();

        _roomId = roomId;
        _config = config;
        _timerThread = timerThread;
        _itemConfig = services.GetRequiredService<ItemRConfig>();

        ColliderCatalog = services.GetRequiredService<InternalColliders>();
        ItemCatalog = services.GetRequiredService<ItemCatalog>();
        InternalEnemyData = services.GetRequiredService<InternalEnemyData>();
        Logger = services.GetRequiredService<ILogger<Room>>();

        _level = level;

        _projectiles = [];

        _players = [];
        _gameObjectIds = [];
        DuplicateEntities = [];
        _killedObjects = [];
        _enemies = [];

        if (LevelInfo.Type == LevelType.Unknown)
        {
            Planes = [];
            _entities = [];
            _colliders = [];

            return;
        }

        Planes = LevelInfo.LoadPlanes(_config);
        _entities = this.LoadEntities(services);
        _colliders = this.LoadTerrainColliders();

        foreach (var type in UnknownEntities.Values.SelectMany(x => x).Distinct().Order())
            Logger.LogWarning("Could not find synced entity for {EntityType}", type);

        foreach (var gameObjectId in Planes.Values
                     .Select(x => x.GameObjects)
                     .SelectMany(x => x.Keys)
                )
            _gameObjectIds.Add(gameObjectId);

        foreach (var component in _entities.Values.SelectMany(x => x))
            component.InitializeComponent();

        foreach (var component in _entities.Values.SelectMany(x => x))
            component.DelayedComponentInitialization();

        foreach (var component in _entities.Values.SelectMany(x => x))
        {
            if (component.Name == config.EnemyComponentName && !component.ParentPlane.Equals("TemplatePlane"))
            {
                var enemy = GenerateEnemy(component.PrefabName, component.Id, (EnemyControllerComp) component);

                if (enemy != null)
                    _enemies.Add(component.Id, enemy);
            }
            if (component.Name == config.BreakableComponentName)
            {
                var breakable = (BreakableEventControllerComp)component;
                breakable.PostInit();
            }
        }

        var spawnPoints = GetEntitiesFromType<SpawnPointComp>();

        _defaultSpawn = spawnPoints.MinBy(p => p.Index);

        if (_defaultSpawn == null)
            Logger.LogError("Could not find default spawn for level: {RoomId} ({RoomName})",
                LevelInfo.LevelId, LevelInfo.Name);

        TimeOffset = GetTime.GetCurrentUnixMilliseconds();
        Start();
    }

    public override void OnTick()
    {
        var entitiesCopy = _entities.Values.SelectMany(s => s).ToList();
        var projectilesCopy = _projectiles.Values.ToList();
        var enemiesCopy = _enemies.Values.ToList();

        foreach (var entityComponent in entitiesCopy)
            if (!IsObjectKilled(entityComponent.Id))
                entityComponent.Update();

        foreach (var projectile in projectilesCopy)
            projectile.Update();

        foreach (var enemy in enemiesCopy)
            enemy.Update();

        foreach (var player in _players?.Values)
        {
            if (GetTime.GetCurrentUnixMilliseconds() - player.CurrentPing > _config.KickAfterTime)
            {
                player.Remove(Logger);
                return;
            }

            var playerCollider = new PlayerCollider(player);
            playerCollider.IsColliding(false);
        }
    }

    public void AddClient(Player currentPlayer, out JoinReason reason)
    {
        reason = _players.Count > _config.PlayerCap ? JoinReason.Full : JoinReason.Accepted;

        if (LevelInfo.LevelId == -1)
            return;

        if (reason == JoinReason.Accepted)
        {
            lock (_roomLock)
            {
                var gameObjectId = 1;

                while (_gameObjectIds.Contains(gameObjectId.ToString()))
                    gameObjectId++;

                _gameObjectIds.Add(gameObjectId.ToString());

                currentPlayer.TempData.GameObjectId = gameObjectId.ToString();

                _players.Add(gameObjectId.ToString(), currentPlayer);

                this.GroupMemberRoomChanged(currentPlayer);

                currentPlayer.NetState.SendXml("joinOK", $"<pid id='{gameObjectId}' /><uLs />");

                if (LevelInfo.LevelId == 0)
                    return;

                // USER ENTER

                foreach (var roomCharacter in _players.Values)
                {
                    currentPlayer.SendUserEnterDataTo(roomCharacter);

                    if (roomCharacter != currentPlayer)
                        roomCharacter.SendUserEnterDataTo(currentPlayer);
                }
            }

            if (_itemConfig.TrainingGear.TryGetValue(LevelInfo.LevelId, out var trainingGear) && _config.GameVersion == GameVersion.v2014)
                currentPlayer.AddGear(trainingGear, ItemCatalog);
            else if (_itemConfig.TrainingGear2011.TryGetValue(LevelInfo.LevelId, out var gear) && _config.GameVersion >= GameVersion.v2011)
                currentPlayer. AddGear(gear, ItemCatalog);
        }
        else
        {
            currentPlayer.NetState.SendXml("joinKO", $"<error>{reason.GetJoinReasonError()}</error>");
        }
    }

    public void RemoveClient(Player player, bool useOriginalRoom)
    {
        lock (_roomLock)
        {
            _players.Remove(player.GameObjectId);
            _gameObjectIds.Remove(player.GameObjectId);
        }

        if (LevelInfo.LevelId <= 0)
            return;

        if (_players.Count != 0)
        {
            lock (_roomLock)
            {
                foreach (var currentPlayer in _players.Values)
                    player.SendUserGoneDataTo(currentPlayer);

                foreach (var entity in GetEntitiesFromType<TriggerCoopControllerComp>())
                    entity.RemovePhysicalInteractor(player.GameObjectId);
            }

            return;
        }

        if (useOriginalRoom)
        {
            var checkpoints = _entities.Where(x => x.Value.Any(x => x is CheckpointControllerComp)).Select(x => x.Value).SelectMany(x => x);

            foreach (var checkpoint in checkpoints)
                checkpoint.InitializeComponent();
        }
        else
        {
            lock (_level.Lock)
            {
                _level.Rooms.Remove(_roomId);
            }

            lock (_roomLock)
            {
                CleanData();
            }

            Stop();
        }
    }

    public void DumpPlayersToLobby(WorldHandler worldHandler)
    {
        foreach (var player in _players.Values)
            player.DumpToLobby(worldHandler);
    }

    public void SendCharacterInfo(Player player)
    {
        // WHERE TO SPAWN
        var character = player.Character;

        var spawnPoint = GetSpawnPoint(character);
        var coordinates = GetSpawnCoordinates(spawnPoint);

        character.Data.SpawnPositionX = coordinates.X;
        character.Data.SpawnPositionY = coordinates.Y;
        character.Data.SpawnOnBackPlane = spawnPoint.IsOnBackPlane(Logger);

        Logger.LogDebug(
            "Spawning {CharacterName} at object '{Object}' (spawn '{SpawnPoint}') for room id '{NewRoom}'.",
            character.Data.CharacterName,
            spawnPoint.Id != string.Empty ? spawnPoint.Id : "DEFAULT",
            character.LevelData.SpawnPointId != string.Empty ? character.LevelData.SpawnPointId : "DEFAULT",
            character.LevelData.LevelId
        );

        Logger.LogDebug("Position of spawn: {Position}", spawnPoint.Position);

        // CHARACTER DATA

        foreach (var currentPlayer in _players.Values)
        {
            var areDifferentClients = currentPlayer.UserId != player.UserId;

            player.SendCharacterInfoDataTo(currentPlayer,
                areDifferentClients ? CharacterInfoType.Lite : CharacterInfoType.Portals, LevelInfo);

            if (areDifferentClients)
                currentPlayer.SendCharacterInfoDataTo(player, CharacterInfoType.Lite, LevelInfo);
        }
    }

    // Players

    public Player GetPlayerById(string id) =>
        _players.TryGetValue(id, out var value) ? value : null;

    public Player[] GetPlayers() =>
        [.. _players.Values];

    // Colliders

    public void AddCollider(BaseCollider collider)
    {
        lock (_roomLock)
        {
            if (!_colliders.TryAdd(collider.Id, collider))
                _colliders[collider.Id] = collider;
        }
    }

    public void RemoveCollider(string colliderId)
    {
        lock (_roomLock)
            _colliders.Remove(colliderId);
    }

    public BaseCollider GetColliderById(string id) =>
        _colliders.TryGetValue(id, out var value) ? value : null;

    public BaseCollider[] GetColliders() =>
        [.. _colliders.Values];

    // Spawn Points

    public static Vector2Model GetSpawnCoordinates(BaseComponent spawnLocation)
    {
        var x = spawnLocation.Rectangle.X;

        if (x == 0)
            x = spawnLocation.Position.X;

        x -= .25f;

        var y = spawnLocation.Rectangle.Y;

        if (y == 0)
            y = spawnLocation.Position.Y;

        y += .25f;

        x += spawnLocation.Rectangle.Width / 2;
        y += spawnLocation.Rectangle.Height / 2;

        return new Vector2Model()
        {
            X = x,
            Y = y
        };
    }

    public BaseComponent GetSpawnPoint(CharacterModel character)
    {
        var spawnPoints = GetEntitiesFromType<SpawnPointComp>().ToDictionary(x => x.Id, x => x);
        var portals = GetEntitiesFromType<PortalComp>().ToDictionary(x => x.Id, x => x);

        var spawnId = character.LevelData.SpawnPointId;

        if (portals.TryGetValue(spawnId, out var portal))
            if (portal != null)
                return portal;

        if (spawnPoints.TryGetValue(spawnId, out var spawnPoint))
            if (spawnPoint != null)
                return spawnPoint;

        var indexSpawn = spawnPoints.Values.FirstOrDefault(s => s.Index.ToString() == character.LevelData.SpawnPointId);

        return indexSpawn ?? (BaseComponent)_defaultSpawn;
    }

    public BaseComponent GetDefaultSpawnPoint() => _defaultSpawn;

    // Entities

    public void AddEntity(string id, List<BaseComponent> entity)
    {
        lock (_roomLock)
            if (!_entities.TryAdd(id, entity))
                _entities[id] = entity;
    }

    public void RemoveEntity(string id)
    {
        lock (_roomLock)
            _entities.Remove(id);
    }

    public bool ContainsEntity(string id) =>
        _entities.ContainsKey(id);

    public Dictionary<string, List<BaseComponent>> GetEntities() => _entities;

    public T GetEntityFromId<T>(string id) where T : class =>
        _entities.TryGetValue(id, out var entities) ?
            entities.FirstOrDefault(x => x is T and not null) as T :
            null;

    public T[] GetEntitiesFromId<T>(string id) where T : class =>
        _entities.TryGetValue(id, out var entities) ?
            entities.Where(x => x is T and not null).Select(x => x as T).ToArray() :
            [];

    public T[] GetEntitiesFromType<T>() where T : class =>
        typeof(T) == typeof(BaseComponent)
            ? _entities.Values.SelectMany(x => x).ToArray() as T[]
            : _entities.SelectMany(x => x.Value).Where(x => x is T and not null).Select(x => x as T).ToArray();

    // Projectiles

    public void AddProjectile(BaseProjectile projectile)
    {
        lock (_roomLock)
            if (!_projectiles.TryAdd(projectile.ProjectileId, projectile))
                _projectiles[projectile.ProjectileId] = projectile;
    }

    public void RemoveProjectile(string enemyId)
    {
        lock (_roomLock)
            _projectiles.Remove(enemyId);
    }

    public int CreateProjectileId()
    {
        var projectileId = Math.Abs(new Random().Next());
        var containsId = false;


        lock (_roomLock)
            containsId = _gameObjectIds.Contains(projectileId.ToString());

        return containsId ?
            CreateProjectileId() :
            projectileId;
    }

    public void AddRangedProjectile(string ownerId, UnityEngine.Vector3 position, UnityEngine.Vector2 speed,
        float lifeTime, int damage, ItemEffectType effect, bool isGrenade)
    {
        var projectileId = CreateProjectileId();

        var positionModel = new Vector3Model()
        {
            X = position.x,
            Y = position.y,
            Z = position.z
        };

        var aiProjectile = new AIProjectile(
            this, ownerId, projectileId.ToString(), positionModel, speed.x, speed.y,
            lifeTime, _timerThread, damage, effect, _config, ItemCatalog
        );

        this.SendSyncEvent(
            AISyncEventHelper.AILaunchItem(
                ownerId, Time,
                position.x, position.y, position.z,
                speed.x, speed.y,
                lifeTime, projectileId, isGrenade
            )
        );

        AddProjectile(aiProjectile);
    }

    // Killed Entities

    public void AddKilledEntity(string killedEnemy)
    {
        lock (_roomLock)
            _killedObjects.Add(killedEnemy);
    }

    public void RemoveKilledEnemy(string killedEnemy)
    {
        lock (_roomLock)
            _killedObjects.Remove(killedEnemy);
    }

    public bool IsObjectKilled(string id)
    {
        lock (_roomLock)
            return _killedObjects.Contains(id);
    }

    public void KillEntity(Player player, string id)
    {
        lock (_roomLock)
            if (_killedObjects.Contains(id))
                return;

        Logger.LogInformation("Killing object {id}...", id);

        var roomEntities = _entities.Values.SelectMany(s => s).ToList();

        foreach (var destructible in GetEntitiesFromId<IDestructible>(id))
        {
            if (destructible is BaseComponent component)
            {
                destructible.Destroy(player, this, component.Id);

                Logger.LogDebug("Killed destructible {destructible} from GameObject {prefabname} with Id {id}",
                    destructible.GetType().Name, component.PrefabName, component.Id);
            }
        }

        AddKilledEntity(id);
    }

    // Enemies

    public void AddEnemy(BaseEnemy enemy)
    {
        lock (_roomLock)
            _enemies.Add(enemy.Id, enemy);
    }

    public void RemoveEnemy(string enemyId)
    {
        lock (_roomLock)
            _enemies.Remove(enemyId);

        RemoveCollider(enemyId);
    }

    public BaseEnemy[] GetEnemies() =>
        [.. _enemies.Values];

    public BaseEnemy GetEnemy(string id) => _enemies.TryGetValue(id, out var value) ? value : null;

    public bool ContainsEnemy(string enemyId) =>
        _enemies.ContainsKey(enemyId);

    public BaseEnemy GenerateEnemy(string enemyPrefab, string entityId, EnemyControllerComp enemyController)
    {
        if (!InternalEnemyData.EnemyInfoCatalog.TryGetValue(enemyPrefab, out var enemyModel))
        {
            Logger.LogError("Could not find enemy with name {EnemyPrefab}! Returning null...", enemyPrefab);
            return null;
        }

        var enemyData = new EnemyData(this, entityId, enemyPrefab, enemyController, enemyModel, _services);
        BaseEnemy enemy;

        switch (enemyModel.AiType)
        {
            case AiType.Behavior:
                enemy = new BehaviorEnemy(enemyData);
                break;
            case AiType.State:
                enemy = new AIStateEnemy(enemyData);
                break;
            default:
                Logger.LogError("No enemy generator found with type: '{Type}' for enemy '{EnemyName}'. Returning null...",
                    enemyModel.AiType, enemyPrefab
                );
                return null;
        }

        enemy.Initialize();

        AddEnemy(enemy);

        return enemy;
    }

    // Cleanup

    private void CleanData()
    {
        _gameObjectIds.Clear();
        _killedObjects.Clear();

        _players.Clear();
        _colliders.Clear();

        Planes.Clear();
        UnknownEntities.Clear();
        _enemies.Clear();
        DuplicateEntities.Clear();

        _entities.Clear();
        _projectiles.Clear();
    }

    public override string ToString() =>
        $"{LevelInfo.LevelId}_{_roomId}";
}
