using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Entity;
using Server.Reawakened.Entities.Interfaces;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Rooms.Enums;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using WorldGraphDefines;
using Timer = Server.Base.Timers.Timer;

namespace Server.Reawakened.Rooms;

public class Room : Timer
{
    private readonly object _roomLock;

    private readonly int _roomId;
    private readonly ServerRConfig _config;
    private readonly Level _level;

    public HashSet<int> GameObjectIds;
    public HashSet<int> KilledObjects;

    public Dictionary<int, Player> Players;
    public Dictionary<int, List<BaseComponent>> Entities;
    public Dictionary<int, ProjectileEntity> Projectiles;
    public Dictionary<int, BaseCollider> Colliders;
    public ILogger<Room> Logger;

    public Dictionary<string, PlaneModel> Planes;
    public Dictionary<int, List<string>> UnknownEntities;

    public SpawnPointComp DefaultSpawn { get; set; }
    public SpawnPointComp CheckpointSpawn { get; set; }
    public int CheckpointId { get; set; }
    public LevelInfo LevelInfo => _level.LevelInfo;
    public long TimeOffset { get; set; }
    public float Time => (float)((GetTime.GetCurrentUnixMilliseconds() - TimeOffset) / 1000.0);

    public IServiceProvider Services { get; }

    public int ProjectileCount;

    public Room(
        int roomId, Level level, ServerRConfig config, TimerThread timerThread,
        IServiceProvider services, ILogger<Room> logger
    ) : base(TimeSpan.Zero, TimeSpan.FromSeconds(1.0 / config.RoomTickRate), 0, timerThread)
    {
        _roomLock = new object();
        KilledObjects = [];

        _roomId = roomId;
        _config = config;
        Services = services;
        Logger = logger;
        _level = level;

        ResetCheckpoints();

        Players = [];
        GameObjectIds = [];

        if (LevelInfo.Type == LevelType.Unknown)
            return;

        Planes = LevelInfo.LoadPlanes(_config);
        Entities = this.LoadEntities(services, out UnknownEntities);
        Projectiles = [];

        Colliders = this.LoadColliders();

        foreach (var gameObjectId in Planes.Values
                     .Select(x => x.GameObjects.Values)
                     .SelectMany(x => x)
                     .Select(x => x.ObjectInfo.ObjectId)
                )
            GameObjectIds.Add(gameObjectId);

        foreach (var component in Entities.Values.SelectMany(x => x))
            component.InitializeComponent();

        var spawnPoints = this.GetComponentsOfType<SpawnPointComp>();

        DefaultSpawn = spawnPoints.Values.MinBy(p => p.Index);

        if (DefaultSpawn == null)
            Logger.LogError("Could not find default spawn for level: {RoomId} ({RoomName})",
                LevelInfo.LevelId, LevelInfo.Name);

        TimeOffset = GetTime.GetCurrentUnixMilliseconds();
        Start();
    }

    public void ResetCheckpoints()
    {
        CheckpointSpawn = null;
        CheckpointId = 0;
    }

    public override void OnTick()
    {
        var entitiesCopy = Entities.Values.SelectMany(s => s).ToList();
        var projectilesCopy = Projectiles.Values.ToList();

        foreach (var entityComponent in entitiesCopy)
        {
            if (!IsObjectKilled(entityComponent.Id))
                entityComponent.Update();
        }

        foreach (var projectileComponent in projectilesCopy)
            projectileComponent.Update();

        foreach (var player in Players.Values.Where(
                     player => GetTime.GetCurrentUnixMilliseconds() - player.CurrentPing > _config.KickAfterTime
                 ))
            player.Remove(Logger);
    }

    public bool IsObjectKilled(int objectId)
    {
        lock (_roomLock)
            return KilledObjects.Contains(objectId);
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
            var gameObjectId = 1;

            while (GameObjectIds.Contains(gameObjectId))
                gameObjectId++;

            GameObjectIds.Add(gameObjectId);

            currentPlayer.TempData.GameObjectId = gameObjectId;

            Players.Add(gameObjectId, currentPlayer);

            GroupMemberRoomChanged(currentPlayer);

            currentPlayer.NetState.SendXml("joinOK", $"<pid id='{gameObjectId}' /><uLs />");

            if (LevelInfo.LevelId == 0)
                return;

            if (currentPlayer.TempData.ArenaModel == null)
                currentPlayer.TempData.ArenaModel = new ArenaModel();

            // USER ENTER

            foreach (var roomCharacter in Players.Values)
            {
                currentPlayer.SendUserEnterDataTo(roomCharacter);

                if (roomCharacter != currentPlayer)
                    roomCharacter.SendUserEnterDataTo(currentPlayer);
            }
        }
        else
        {
            currentPlayer.NetState.SendXml("joinKO", $"<error>{reason.GetJoinReasonError()}</error>");
        }
    }

    public void RemoveClient(Player player, bool useOriginalRoom)
    {
        Players.Remove(player.GameObjectId);
        GameObjectIds.Remove(player.GameObjectId);

        if (LevelInfo.LevelId <= 0)
            return;

        if (Players.Count != 0)
        {
            foreach (var currentPlayer in Players.Values)
                player.SendUserGoneDataTo(currentPlayer);

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
            player.TempData.ArenaModel = null;
            _level.Rooms.Remove(_roomId);
            Stop();
        }
    }

    public void SendCharacterInfo(Player player)
    {
        // WHERE TO SPAWN
        var character = player.Character;

        var spawnPoint = GetSpawnPoint(character);
        var coords = GetSpawnCoords(spawnPoint);

        character.Data.SpawnPositionX = coords.X;
        character.Data.SpawnPositionY = coords.Y;

        if (spawnPoint.ParentPlane == "Plane1")
            character.Data.SpawnOnBackPlane = true;
        else if (spawnPoint.ParentPlane == "Plane0")
            character.Data.SpawnOnBackPlane = false;
        else
            Logger.LogWarning("Unknown plane for portal: {PortalPlane}", spawnPoint.ParentPlane);

        Logger.LogDebug(
            "Spawning {CharacterName} at object '{Object}' (spawn '{SpawnPoint}') for room id '{NewRoom}'.",
            character.Data.CharacterName,
            spawnPoint.Id != 0 ? spawnPoint.Id : "DEFAULT",
            character.LevelData.SpawnPointId != 0 ? character.LevelData.SpawnPointId : "DEFAULT",
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

    public static Vector2Model GetSpawnCoords(BaseComponent spawnLocation)
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

        var indexSpawn = spawnPoints.Values.FirstOrDefault(s => s.Index == character.LevelData.SpawnPointId);

        return indexSpawn ?? (BaseComponent) DefaultSpawn;
    }

    public void DumpPlayersToLobby()
    {
        foreach (var player in Players.Values)
            player.DumpToLobby();
    }

    public void Kill(int objectId)
    {
        lock (_roomLock)
            if (KilledObjects.Contains(objectId))
                return;


        Logger.LogInformation("Killing object {id}...", objectId);

        var roomEntities = Entities.Values.SelectMany(s => s).ToList();

        foreach (var component in roomEntities.Where(c => c is IKillable))
            if (component.Id == objectId)
            {
                var kbComp = component as IKillable;

                kbComp.ObjectKilled();

                Logger.LogDebug("Killed component {component} from GameObject {prefabname} with Id {id}",
                    component.GetType().Name, component.PrefabName, component.Id);
            }

        lock (_roomLock)
            KilledObjects.Add(objectId);
    }

    public string GetRoomName() =>
    $"{LevelInfo.LevelId}_{_roomId}";
}
