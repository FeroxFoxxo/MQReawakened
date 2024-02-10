using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Entity;
using Server.Reawakened.Entities.Entity.Enemies;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Arenas;
using Server.Reawakened.Rooms.Enums;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.BundlesInternal;
using WorldGraphDefines;
using Timer = Server.Base.Timers.Timer;

namespace Server.Reawakened.Rooms;

public class Room : Timer
{
    private readonly object _roomLock;

    private readonly int _roomId;
    private readonly ServerRConfig _config;
    private readonly Level _level;

    public HashSet<string> GameObjectIds;

    public Dictionary<string, Player> Players;
    public Dictionary<string, List<BaseComponent>> Entities;
    public Dictionary<string, TicklyEntity> Projectiles;
    public Dictionary<string, BaseCollider> Colliders;
    public ILogger<Room> Logger;

    public Dictionary<string, PlaneModel> Planes;
    public Dictionary<string, List<string>> UnknownEntities;
    public Dictionary<string, Enemy> Enemies;
    public Dictionary<string, List<List<BaseComponent>>> DuplicateEntities;

    public SpawnPointComp DefaultSpawn { get; set; }
    public InternalColliders ColliderCatalog;

    public CheckpointControllerComp LastCheckpoint { get; set; }

    public LevelInfo LevelInfo => _level.LevelInfo;
    public long TimeOffset { get; set; }
    public float Time => (float)((GetTime.GetCurrentUnixMilliseconds() - TimeOffset) / 1000.0);

    public IServiceProvider Services { get; }

    public Room(
        int roomId, Level level, ServerRConfig config, TimerThread timerThread,
        IServiceProvider services, ILogger<Room> logger, InternalColliders colliderCatalog
    ) : base(TimeSpan.Zero, TimeSpan.FromSeconds(1.0 / config.RoomTickRate), 0, timerThread)
    {
        _roomLock = new object();

        _roomId = roomId;
        _config = config;
        Services = services;
        Logger = logger;
        ColliderCatalog = colliderCatalog;
        _level = level;

        Players = [];
        GameObjectIds = [];
        DuplicateEntities = [];

        if (LevelInfo.Type == LevelType.Unknown)
            return;

        Planes = LevelInfo.LoadPlanes(_config);
        Entities = this.LoadEntities(services);
        Projectiles = [];
        Colliders = this.LoadTerrainColliders();
        Enemies = [];

        foreach (var type in UnknownEntities.Values.SelectMany(x => x).Distinct().Order())
            Logger.LogWarning("Could not find synced entity for {EntityType}", type);

        foreach (var gameObjectId in Planes.Values
                     .Select(x => x.GameObjects)
                     .SelectMany(x => x.Keys)
                )
            GameObjectIds.Add(gameObjectId);

        foreach (var component in Entities.Values.SelectMany(x => x))
        {
            component.InitializeComponent();
        }

        foreach (var component in Entities.Values.SelectMany(x => x))
        {
            component.DelayedComponentInitialization();
        }

        foreach (var component in Entities.Values.SelectMany(x => x))
        {
            if (component.Name == config.EnemyComponentName)
            {
                // Move the name switcher out of ServerRConfig when the enemy xml is made.
                switch (component.PrefabName)
                {
                    case string bird when bird.Contains(config.EnemyNameSearch[0]):
                        Enemies.Add(component.Id, new EnemyBird(this, component.Id, component));
                        break;
                    case string fish when fish.Contains(config.EnemyNameSearch[1]):
                        Enemies.Add(component.Id, new EnemyFish(this, component.Id, component));
                        break;
                    case string spider when spider.Contains(config.EnemyNameSearch[2]):
                        Enemies.Add(component.Id, new EnemySpider(this, component.Id, component));
                        break;
                    case string bathog when bathog.Contains(config.EnemyNameSearch[3]):
                        Enemies.Add(component.Id, new EnemyBathog(this, component.Id, component));
                        break;
                    case string bomber when bomber.Contains(config.EnemyNameSearch[4]):
                        Enemies.Add(component.Id, new EnemyBomber(this, component.Id, component));
                        break;
                    case string crawler when crawler.Contains(config.EnemyNameSearch[5]):
                        Enemies.Add(component.Id, new EnemyCrawler(this, component.Id, component));
                        break;
                    case string dragon when dragon.Contains(config.EnemyNameSearch[6]):
                        Enemies.Add(component.Id, new EnemyDragon(this, component.Id, component));
                        break;
                    case string grenadier when grenadier.Contains(config.EnemyNameSearch[7]):
                        Enemies.Add(component.Id, new EnemyGrenadier(this, component.Id, component));
                        break;
                    case string orchid when orchid.Contains(config.EnemyNameSearch[8]):
                        Enemies.Add(component.Id, new EnemyOrchid(this, component.Id, component));
                        break;
                    case string pincer when pincer.Contains(config.EnemyNameSearch[9]):
                        Enemies.Add(component.Id, new EnemyPincer(this, component.Id, component));
                        break;
                    case string stomper when stomper.Contains(config.EnemyNameSearch[10]):
                        Enemies.Add(component.Id, new EnemyStomper(this, component.Id, component));
                        break;
                    case string vespid when vespid.Contains(config.EnemyNameSearch[11]):
                        Enemies.Add(component.Id, new EnemyVespid(this, component.Id, component));
                        break;
                }
            }
            if (component.Name == config.BreakableComponentName)
            {
                var breakable = (BreakableEventControllerComp)component;
                breakable.PostInit();
            }

        }

        var spawnPoints = this.GetComponentsOfType<SpawnPointComp>();

        DefaultSpawn = spawnPoints.Values.MinBy(p => p.Index);

        if (DefaultSpawn == null)
            Logger.LogError("Could not find default spawn for level: {RoomId} ({RoomName})",
                LevelInfo.LevelId, LevelInfo.Name);

        TimeOffset = GetTime.GetCurrentUnixMilliseconds();
        Start();
    }

    public override void OnTick()
    {
        var entitiesCopy = Entities.Values.SelectMany(s => s).ToList();
        var projectilesCopy = Projectiles.Values.ToList();
        var enemiesCopy = Enemies.Values.ToList();

        foreach (var entityComponent in entitiesCopy)
        {
            //if (!IsObjectKilled(entityComponent.Id))
            entityComponent.Update();
        }

        foreach (var projectile in projectilesCopy)
            projectile.Update();

        foreach (var enemy in enemiesCopy)
            enemy.Update();

        foreach (var player in Players.Values.Where(
                     player => GetTime.GetCurrentUnixMilliseconds() - player.CurrentPing > _config.KickAfterTime
                 ))
            player.Remove(Logger);
    }

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

            if (_config.TrainingGear.TryGetValue(LevelInfo.LevelId, out var trainingGear))
            {
                var item = currentPlayer.DatabaseContainer.ItemCatalog.GetItemFromPrefabName(trainingGear);

                if (item != null)
                {
                    if (!currentPlayer.Character.Data.Inventory.Items.ContainsKey(item.ItemId))
                    {
                        currentPlayer.AddItem(item, 1);
                        currentPlayer.SendUpdatedInventory(false);
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
            }

            return;
        }

        if (useOriginalRoom)
        {
            var checkpoints = Entities.Where(x => x.Value.Any(x => x is CheckpointControllerComp)).Select(x => x.Value).SelectMany(x => x);

            foreach (var checkpoint in checkpoints)
                checkpoint.InitializeComponent();
        }
        else
        {
            lock (_level.Lock)
            {
                _level.Rooms.Remove(_roomId);
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
        var spawnPoints = this.GetComponentsOfType<SpawnPointComp>();
        var portals = this.GetComponentsOfType<PortalControllerComp>();

        var spawnId = character.LevelData.SpawnPointId;

        if (portals.TryGetValue(spawnId, out var portal))
            if (portal != null)
                return portal;

        if (spawnPoints.TryGetValue(spawnId, out var spawnPoint))
            if (spawnPoint != null)
                return spawnPoint;

        var indexSpawn = spawnPoints.Values.FirstOrDefault(s => s.Index.ToString() == character.LevelData.SpawnPointId);

        return indexSpawn ?? (BaseComponent) DefaultSpawn;
    }

    public void DumpPlayersToLobby()
    {
        foreach (var player in Players.Values)
            player.DumpToLobby();
    }

    public string GetRoomName() =>
    $"{LevelInfo.LevelId}_{_roomId}";
}
