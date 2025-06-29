using A2m.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Controller;
using Server.Reawakened.Entities.Components.GameObjects.Checkpoints;
using Server.Reawakened.Entities.Components.GameObjects.Global;
using Server.Reawakened.Entities.Components.GameObjects.InterObjs.Interfaces;
using Server.Reawakened.Entities.Components.GameObjects.Spawners;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using Server.Reawakened.Entities.Enemies.Extensions;
using Server.Reawakened.Entities.Projectiles;
using Server.Reawakened.Entities.Projectiles.Abstractions;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Enums;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;
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

    public ItemCatalog ItemCatalog;
    public InternalColliders ColliderCatalog;

    public WorldHandler World;

    public CheckpointControllerComp LastCheckpoint { get; set; }

    public LevelInfo LevelInfo => _level.LevelInfo;

    public long TimeOffset { get; set; }
    public float Time => (float)((GetTime.GetCurrentUnixMilliseconds() - TimeOffset) / 1000.0);
    public float _lastTickTime;

    public float DeltaTime => Time - _lastTickTime;

    public bool IsOpen;

    public Room(int roomId, Level level, IServiceProvider services, TimerThread timerThread, ServerRConfig config) :
        base(TimeSpan.Zero, TimeSpan.FromSeconds(1.0 / config.RoomTickRate), 0, timerThread)
    {
        _roomLock = new object();

        _roomId = roomId;
        _config = config;
        _timerThread = timerThread;
        _lastTickTime = Time;

        IsOpen = true;

        _itemConfig = services.GetRequiredService<ItemRConfig>();
        ColliderCatalog = services.GetRequiredService<InternalColliders>();
        ItemCatalog = services.GetRequiredService<ItemCatalog>();
        Logger = services.GetRequiredService<ILogger<Room>>();
        World = services.GetRequiredService<WorldHandler>();

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

        Planes = LevelInfo.LoadPlanes(this, _config);
        _entities = this.LoadEntities(services);
        _colliders = this.LoadTerrainColliders();

        _defaultSpawn = GetEntitiesFromType<SpawnPointComp>().MinBy(p => p.Index);

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
            if (GetTime.GetCurrentUnixMilliseconds() - player.TempData.CurrentPing > _config.KickAfterTime)
            {
                player.Remove(Logger);
                continue;
            }
            else
            {
                var playerCollider = new PlayerCollider(player);
                playerCollider.IsColliding(false);
            }
        }

        _lastTickTime = Time;
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

            if (_itemConfig.TrainingGear.TryGetValue(LevelInfo.LevelId, out var trainingGear) && _config.GameVersion >= GameVersion.vEarly2014)
                currentPlayer.AddGear(trainingGear, ItemCatalog);
            else if (_itemConfig.TrainingGear2011.TryGetValue(LevelInfo.LevelId, out var gear) && _config.GameVersion >= GameVersion.v2011)
                foreach (var item in gear)
                    currentPlayer.AddGear(item, ItemCatalog);
        }
        else
        {
            currentPlayer.NetState.SendXml("joinKO", $"<error>{reason.GetJoinReasonError()}</error>");
        }
    }

    public void RemoveClient(Player player)
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
                    entity.RemovePhysicalInteractor(player, player.GameObjectId);
            }

            return;
        }

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

    public void DumpPlayersToLobby(WorldHandler worldHandler)
    {
        foreach (var player in _players.Values)
            player.DumpToLobby(worldHandler);
    }

    public void SendCharacterInfo(Player player)
    {
        SetPlayerPosition(player.Character);

        foreach (var currentPlayer in _players.Values)
        {
            var areDifferentClients = currentPlayer.UserId != player.UserId;

            player.SendCharacterInfoDataTo(currentPlayer,
                areDifferentClients ? CharacterInfoType.Lite : CharacterInfoType.Portals, LevelInfo);

            if (areDifferentClients)
                currentPlayer.SendCharacterInfoDataTo(player, CharacterInfoType.Lite, LevelInfo);
        }
    }

    public void SetPlayerPosition(CharacterModel character)
    {
        var spawnPoint = GetSpawnPoint(character);
        var coordinates = GetSpawnCoordinates(spawnPoint);

        character.Write.SpawnPositionX = coordinates.x;
        character.Write.SpawnPositionY = coordinates.y;
        character.Write.SpawnOnBackPlane = spawnPoint.IsOnBackPlane(Logger);

        Logger.LogDebug(
            "Spawning {CharacterName} at object '{Object}' (spawn '{SpawnPoint}') for room id '{NewRoom}'.",
            character.CharacterName,
            spawnPoint.Id != string.Empty ? spawnPoint.Id : "DEFAULT",
            character.SpawnPointId != string.Empty ? character.SpawnPointId : "DEFAULT",
            character.LevelId
        );

        Logger.LogDebug("Position of spawn: {Position}", spawnPoint.Position);
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
            _colliders.TryAdd(collider.Id, collider);
    }

    public void OverwriteCollider(BaseCollider collider)
    {
        lock (_roomLock)
            _colliders[collider.Id] = collider;
    }

    public void RemoveCollider(string colliderId)
    {
        lock (_roomLock)
            _colliders.Remove(colliderId);
    }

    public void ToggleCollider(string colliderId, bool active)
    {
        if (_colliders.TryGetValue(colliderId, out var collider))
            lock (_roomLock)
                collider.Active = active;
    }

    public BaseCollider GetColliderById(string id) =>
        _colliders.TryGetValue(id, out var value) ? value : null;

    public BaseCollider[] GetColliders() =>
        [.. _colliders.Values];

    // Spawn Points

    public static Vector2 GetSpawnCoordinates(BaseComponent spawnLocation)
    {
        var rect = spawnLocation.Rectangle;
        var pos = spawnLocation.Position;

        return new Vector2()
        {
            x = (rect.X == 0 ? pos.X : rect.X) + spawnLocation.Rectangle.Width / 2 - .5f,
            y = (rect.Y == 0 ? pos.Y : rect.Y) + spawnLocation.Rectangle.Height / 2 + .25f
        };
    }

    public BaseComponent GetSpawnPoint(CharacterModel character)
    {
        var spawnPoints = GetEntitiesFromType<SpawnPointComp>().ToDictionary(x => x.Id, x => x);
        var portals = GetEntitiesFromType<PortalComp>().ToDictionary(x => x.Id, x => x);

        var spawnId = character.SpawnPointId;

        if (portals.TryGetValue(spawnId, out var portal))
            if (portal != null)
                return portal;

        if (spawnPoints.TryGetValue(spawnId, out var spawnPoint))
            if (spawnPoint != null)
                return spawnPoint;

        var indexSpawn = spawnPoints.Values.FirstOrDefault(s => s.Index.ToString() == character.SpawnPointId);

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

    public object GetEntityFromId(string id, Type t) =>
        _entities.TryGetValue(id, out var entities) ?
            entities.FirstOrDefault(x => x != null && t.IsInstanceOfType(x)) :
            null;

    public IEnemyController GetEnemyFromId(string id)
    {
        var enemy = GetEntityFromId<EnemyControllerComp>(id);
        return enemy != null ? enemy : GetEntityFromId<ArmoredEnemyControllerComp>(id);
    }

    public T[] GetEntitiesFromId<T>(string id) where T : class =>
        _entities.TryGetValue(id, out var entities) ?
            [.. entities.Where(x => x is T and not null).Select(x => x as T)] :
            [];

    public T[] GetEntitiesFromType<T>() where T : class =>
        typeof(T) == typeof(BaseComponent)
            ? _entities.Values.SelectMany(x => x).ToArray() as T[]
            : [.. _entities.SelectMany(x => x.Value).Where(x => x is T and not null).Select(x => x as T)];

    // Projectiles

    public void AddProjectile(BaseProjectile projectile)
    {
        lock (_roomLock)
            _projectiles.Add(projectile.ProjectileId, projectile);
    }

    public void SetProjectile(BaseProjectile projectile)
    {
        lock (_roomLock)
            _projectiles[projectile.ProjectileId] = projectile;
    }

    public void RemoveProjectile(string projectileId)
    {
        lock (_roomLock)
            _projectiles.Remove(projectileId);
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

    public void AddRangedProjectile(string ownerId, Vector3 position, Vector2 speed,
        float lifeTime, int damage, ItemEffectType effect, bool isGrenade)
    {
        var projectileId = CreateProjectileId();

        var aiProjectile = new AIProjectile(
            this, ownerId, projectileId.ToString(), position, speed,
            lifeTime, _timerThread, damage, effect, isGrenade, _config, ItemCatalog, _itemConfig
        );

        this.SendSyncEvent(
            AISyncEventHelper.AILaunchItem(
                ownerId, Time, position, speed, lifeTime, projectileId, isGrenade
            )
        );

        AddProjectile(aiProjectile);
    }

    // Killed Entities

    public void AddKilledEnemy(string killedEnemy)
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

    public void KillEntity(string id)
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
                destructible.Destroy(this, component.Id);

                Logger.LogDebug("Killed destructible {Destructible} from game object '{PrefabName}' ({Id})",
                    destructible.GetType().Name, component.PrefabName, component.Id);
            }
        }

        AddKilledEnemy(id);
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

    // Cleanup
    private void CleanData()
    {
        // Deallocate rooms.
        foreach (var entity in _entities)
        {
            foreach (var component in entity.Value)
            {
                component.Entity.Room = null;
            }
        }

        IsOpen = false;

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
