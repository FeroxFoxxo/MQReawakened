using A2m.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Entity;
using Server.Reawakened.Entities.Entity.Enemies.BehaviorEnemies;
using Server.Reawakened.Entities.Interfaces;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Rooms.Enums;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using WorldGraphDefines;
using Timer = Server.Base.Timers.Timer;

namespace Server.Reawakened.Rooms;

public class Room : Timer
{
    private readonly object _roomLock;

    private readonly int _roomId;
    private readonly Level _level;

    public HashSet<string> GameObjectIds;
    public HashSet<string> KilledObjects;

    public Dictionary<string, Player> Players;
    public Dictionary<string, TicklyEntity> Projectiles;
    public Dictionary<string, BaseCollider> Colliders;

    public ILogger<Room> Logger;

    public Dictionary<string, PlaneModel> Planes;
    public Dictionary<string, List<string>> UnknownEntities;
    public Dictionary<string, Enemy> Enemies;
    public Dictionary<string, List<List<BaseComponent>>> DuplicateEntities;

    private readonly Dictionary<string, List<BaseComponent>> _entities;

    public SpawnPointComp DefaultSpawn { get; set; }

    private readonly ServerRConfig _config;

    public ItemCatalog ItemCatalog;
    public InternalColliders ColliderCatalog;

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

        ColliderCatalog = services.GetRequiredService<InternalColliders>();
        ItemCatalog = services.GetRequiredService<ItemCatalog>();
        Logger = services.GetRequiredService<ILogger<Room>>();

        _level = level;

        Players = [];
        GameObjectIds = [];
        DuplicateEntities = [];
        KilledObjects = [];
        Projectiles = [];
        Enemies = [];

        if (LevelInfo.Type == LevelType.Unknown)
        {
            Planes = [];
            _entities = [];
            Colliders = [];

            return;
        }

        Planes = LevelInfo.LoadPlanes(_config);
        _entities = this.LoadEntities(services);
        Colliders = this.LoadTerrainColliders();

        foreach (var type in UnknownEntities.Values.SelectMany(x => x).Distinct().Order())
            Logger.LogWarning("Could not find synced entity for {EntityType}", type);

        foreach (var gameObjectId in Planes.Values
                     .Select(x => x.GameObjects)
                     .SelectMany(x => x.Keys)
                )
            GameObjectIds.Add(gameObjectId);

        foreach (var component in _entities.Values.SelectMany(x => x))
            component.InitializeComponent();

        foreach (var component in _entities.Values.SelectMany(x => x))
            component.DelayedComponentInitialization();

        foreach (var component in _entities.Values.SelectMany(x => x))
        {
            if (component.Name == config.EnemyComponentName && !component.ParentPlane.Equals("TemplatePlane"))
            {
                // Move the name switcher out of ServerRConfig when the enemy xml is made.
                switch (component.PrefabName)
                {
                    case string bird when bird.Contains(config.EnemyNameSearch[0]):
                        Enemies.Add(component.Id, new EnemyBird(this, component.Id, component.PrefabName, (EnemyControllerComp)component, services));
                        break;
                    case string fish when fish.Contains(config.EnemyNameSearch[1]):
                        Enemies.Add(component.Id, new EnemyFish(this, component.Id, component.PrefabName, (EnemyControllerComp)component, services));
                        break;
                    case string spider when spider.Contains(config.EnemyNameSearch[2]):
                        Enemies.Add(component.Id, new EnemySpider(this, component.Id, component.PrefabName, (EnemyControllerComp)component, services));
                        break;
                    case string bathog when bathog.Contains(config.EnemyNameSearch[3]):
                        Enemies.Add(component.Id, new EnemyBathog(this, component.Id, component.PrefabName, (EnemyControllerComp)component, services));
                        break;
                    case string bomber when bomber.Contains(config.EnemyNameSearch[4]):
                        Enemies.Add(component.Id, new EnemyBomber(this, component.Id, component.PrefabName, (EnemyControllerComp)component, services));
                        break;
                    case string crawler when crawler.Contains(config.EnemyNameSearch[5]):
                        Enemies.Add(component.Id, new EnemyCrawler(this, component.Id, component.PrefabName, (EnemyControllerComp)component, services));
                        break;
                    case string dragon when dragon.Contains(config.EnemyNameSearch[6]):
                        Enemies.Add(component.Id, new EnemyDragon(this, component.Id, component.PrefabName, (EnemyControllerComp)component, services));
                        break;
                    case string grenadier when grenadier.Contains(config.EnemyNameSearch[7]):
                        Enemies.Add(component.Id, new EnemyGrenadier(this, component.Id, component.PrefabName, (EnemyControllerComp)component, services));
                        break;
                    case string orchid when orchid.Contains(config.EnemyNameSearch[8]):
                        Enemies.Add(component.Id, new EnemyOrchid(this, component.Id, component.PrefabName, (EnemyControllerComp)component, services));
                        break;
                    case string pincer when pincer.Contains(config.EnemyNameSearch[9]):
                        Enemies.Add(component.Id, new EnemyPincer(this, component.Id, component.PrefabName, (EnemyControllerComp)component, services));
                        break;
                    case string stomper when stomper.Contains(config.EnemyNameSearch[10]):
                        Enemies.Add(component.Id, new EnemyStomper(this, component.Id, component.PrefabName, (EnemyControllerComp)component, services));
                        break;
                    case string vespid when vespid.Contains(config.EnemyNameSearch[11]):
                        Enemies.Add(component.Id, new EnemyVespid(this, component.Id, component.PrefabName, (EnemyControllerComp)component, services));
                        break;
                    case string spiderling when spiderling.Contains(config.EnemyNameSearch[12]):
                        Enemies.Add(component.Id, new EnemySpiderling(this, component.Id, component.PrefabName, (EnemyControllerComp)component, services));
                        break;
                    case string teaserSpiderBoss when teaserSpiderBoss.Contains(config.EnemyNameSearch[13]):
                        Enemies.Add(component.Id, new EnemyTeaserSpiderBoss(this, component.Id, component.PrefabName, (EnemyControllerComp)component, services));
                        break;
                    case string spiderBoss when spiderBoss.Contains(config.EnemyNameSearch[14]):
                        Enemies.Add(component.Id, new EnemySpiderBoss(this, component.Id, component.PrefabName, (EnemyControllerComp)component, services));
                        break;
                }
            }
            if (component.Name == config.BreakableComponentName)
            {
                var breakable = (BreakableEventControllerComp)component;
                breakable.PostInit();
            }
        }

        var spawnPoints = GetEntitiesFromType<SpawnPointComp>();

        DefaultSpawn = spawnPoints.MinBy(p => p.Index);

        if (DefaultSpawn == null)
            Logger.LogError("Could not find default spawn for level: {RoomId} ({RoomName})",
                LevelInfo.LevelId, LevelInfo.Name);

        TimeOffset = GetTime.GetCurrentUnixMilliseconds();
        Start();
    }

    public override void OnTick()
    {
        var entitiesCopy = _entities.Values.SelectMany(s => s).ToList();
        var projectilesCopy = Projectiles.Values.ToList();
        var enemiesCopy = Enemies.Values.ToList();

        foreach (var entityComponent in entitiesCopy)
            if (!IsObjectKilled(entityComponent.Id))
                entityComponent.Update();

        foreach (var projectile in projectilesCopy)
            projectile.Update();

        foreach (var enemy in enemiesCopy)
            enemy.Update();

        foreach (var player in Players?.Values)
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

    public bool AddEntity(string id, List<BaseComponent> entity) => _entities.TryAdd(id, entity);

    public bool RemoveEntity(string id) => _entities.Remove(id);

    public void GroupMemberRoomChanged(Player player)
    {
        if (player.TempData.Group == null)
            return;

        foreach (var groupMember in player.TempData.Group.GetMembers())
        {
            groupMember.SendXt(
                "pm",
                player.CharacterName,
                LevelInfo.Name,
                GetRoomName()
            );
        }
    }

    public void AddClient(Player currentPlayer, out JoinReason reason)
    {
        reason = Players.Count > _config.PlayerCap ? JoinReason.Full : JoinReason.Accepted;

        if (LevelInfo.LevelId == -1)
            return;

        if (reason == JoinReason.Accepted)
        {
            lock (_roomLock)
            {
                var gameObjectId = 1;

                while (GameObjectIds.Contains(gameObjectId.ToString()))
                    gameObjectId++;

                GameObjectIds.Add(gameObjectId.ToString());

                currentPlayer.TempData.GameObjectId = gameObjectId.ToString();

                Players.Add(gameObjectId.ToString(), currentPlayer);

                GroupMemberRoomChanged(currentPlayer);

                currentPlayer.NetState.SendXml("joinOK", $"<pid id='{gameObjectId}' /><uLs />");

                if (LevelInfo.LevelId == 0)
                    return;

                // USER ENTER

                foreach (var roomCharacter in Players.Values)
                {
                    currentPlayer.SendUserEnterDataTo(roomCharacter);

                    if (roomCharacter != currentPlayer)
                        roomCharacter.SendUserEnterDataTo(currentPlayer);
                }
            }

            if (_config.TrainingGear.TryGetValue(LevelInfo.LevelId, out var trainingGear) && _config.GameVersion == GameVersion.v2014)
            {
                var item = ItemCatalog.GetItemFromPrefabName(trainingGear);

                if (item != null)
                {
                    if (!currentPlayer.Character.Data.Inventory.Items.ContainsKey(item.ItemId))
                    {
                        currentPlayer.AddItem(item, 1, ItemCatalog);
                        currentPlayer.SendUpdatedInventory();
                    }
                }
            }
            else if (_config.TrainingGear2011.TryGetValue(LevelInfo.LevelId, out var gear) && _config.GameVersion >= GameVersion.v2011)
            {
                var item = ItemCatalog.GetItemFromPrefabName(gear);

                if (item != null)
                {
                    if (!currentPlayer.Character.Data.Inventory.Items.ContainsKey(item.ItemId))
                    {
                        currentPlayer.AddItem(item, 1, ItemCatalog);
                        currentPlayer.SendUpdatedInventory();
                    }
                }
            }
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
            Players.Remove(player.GameObjectId);
            GameObjectIds.Remove(player.GameObjectId);
        }

        if (LevelInfo.LevelId <= 0)
            return;

        if (Players.Count != 0)
        {
            lock (_roomLock)
            {
                foreach (var currentPlayer in Players.Values)
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

        foreach (var currentPlayer in Players.Values)
        {
            var areDifferentClients = currentPlayer.UserId != player.UserId;

            player.SendCharacterInfoDataTo(currentPlayer,
                areDifferentClients ? CharacterInfoType.Lite : CharacterInfoType.Portals, LevelInfo);

            if (areDifferentClients)
                currentPlayer.SendCharacterInfoDataTo(player, CharacterInfoType.Lite, LevelInfo);
        }
    }

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
        var portals = GetEntitiesFromType<PortalControllerComp>().ToDictionary(x => x.Id, x => x);

        var spawnId = character.LevelData.SpawnPointId;

        if (portals.TryGetValue(spawnId, out var portal))
            if (portal != null)
                return portal;

        if (spawnPoints.TryGetValue(spawnId, out var spawnPoint))
            if (spawnPoint != null)
                return spawnPoint;

        var indexSpawn = spawnPoints.Values.FirstOrDefault(s => s.Index.ToString() == character.LevelData.SpawnPointId);

        return indexSpawn ?? (BaseComponent)DefaultSpawn;
    }

    public void DumpPlayersToLobby(WorldHandler worldHandler)
    {
        foreach (var player in Players.Values)
            player.DumpToLobby(worldHandler);
    }

    public string GetRoomName() =>
        $"{LevelInfo.LevelId}_{_roomId}";

    // Entity Code

    public bool IsObjectKilled(string id)
    {
        lock (_roomLock)
            return KilledObjects.Contains(id);
    }

    public bool ContainsEntity(string id) =>
        _entities.ContainsKey(id);

    public void KillEntity(Player player, string id)
    {
        if (player == null)
            return;

        lock (_roomLock)
            if (KilledObjects.Contains(id))
                return;

        Logger.LogInformation("Killing object {id}...", id);

        var roomEntities = _entities.Values.SelectMany(s => s).ToList();

        foreach (var destructible in GetEntitiesFromId<IDestructible>(id))
        {
            if (destructible is BaseComponent component)
            {
                destructible.Destroy(player, player.Room, component.Id);

                Logger.LogDebug("Killed destructible {destructible} from GameObject {prefabname} with Id {id}",
                    destructible.GetType().Name, component.PrefabName, component.Id);
            }
        }

        lock (_roomLock)
            KilledObjects.Add(id);
    }

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

    public string SetProjectileId()
    {
        var rand = new Random();
        var projectileId = Math.Abs(rand.Next()).ToString();

        while (GameObjectIds.Contains(projectileId))
            projectileId = Math.Abs(rand.Next()).ToString();

        return projectileId;
    }

    private void CleanData()
    {
        GameObjectIds.Clear();
        KilledObjects.Clear();

        Players.Clear();
        Projectiles.Clear();
        Colliders.Clear();

        Planes.Clear();
        UnknownEntities.Clear();
        Enemies.Clear();
        DuplicateEntities.Clear();

        _entities.Clear();
    }
}
