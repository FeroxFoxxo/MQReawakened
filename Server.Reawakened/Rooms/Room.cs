using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Entity;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Enums;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using WorldGraphDefines;
using Timer = Server.Base.Timers.Timer;

namespace Server.Reawakened.Rooms;

public class Room : Timer
{
    private readonly int _roomId;
    private readonly ServerRConfig _config;
    private readonly Level _level;
    public readonly HashSet<int> GameObjectIds;

    public readonly Dictionary<int, Player> Players;
    public readonly Dictionary<int, List<BaseComponent>> Entities;
    public readonly Dictionary<int, ProjectileEntity> Projectiles;
    public readonly Dictionary<int, BaseCollider> Colliders;
    public readonly ILogger<Room> Logger;

    public readonly Dictionary<string, PlaneModel> Planes;
    public readonly Dictionary<int, List<string>> UnknownEntities;

    public SpawnPointComp DefaultSpawn { get; set; }

    public SpawnPointComp CheckpointSpawn { get; set; }
    public int CheckpointId { get; set; }
    public LevelInfo LevelInfo => _level.LevelInfo;
    public long TimeOffset { get; set; }
    public float Time => (float)((GetTime.GetCurrentUnixMilliseconds() - TimeOffset) / 1000.0);
    public int ProjectileCount;

    public Room(
        int roomId, Level level, ServerRConfig config, TimerThread timerThread,
        IServiceProvider services, ILogger<Room> logger
    ) : base(TimeSpan.Zero, TimeSpan.FromSeconds(1.0 / config.RoomTickRate), 0, timerThread)
    {
        _roomId = roomId;
        _config = config;
        Logger = logger;
        _level = level;

        CheckpointSpawn = null;
        CheckpointId = 0;

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

    public override void OnTick()
    {
        var entitiesCopy = Entities.Values.SelectMany(s => s).ToList();
        var projectilesCopy = Projectiles.Values.ToList();
        foreach (var entityComponent in entitiesCopy)
        {
            if (!entityComponent.Disposed)
                entityComponent.Update();
        }

        foreach (var projectileComponent in projectilesCopy)
            projectileComponent.Update();

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

    public void RemoveClient(Player player)
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

        _level.Rooms.Remove(_roomId);
        Stop();
    }

    public void SendCharacterInfo(Player player)
    {
        // WHERE TO SPAWN
        var character = player.Character;

        BaseComponent spawnLocation = null;

        var spawnPoints = this.GetComponentsOfType<SpawnPointComp>();
        var portals = this.GetComponentsOfType<PortalControllerComp>();

        var spawnId = character.LevelData.SpawnPointId;

        if (spawnId != 0)
        {
            if (portals.TryGetValue(spawnId, out var portal))
                spawnLocation = portal;

            if (spawnLocation == null)
            {
                if (spawnPoints.TryGetValue(spawnId, out var spawnPoint))
                {
                    spawnLocation = spawnPoint;
                }
                else
                {
                    var indexSpawn = spawnPoints.Values.FirstOrDefault(s => s.Index == character.LevelData.SpawnPointId);
                    if (indexSpawn != null)
                        spawnLocation = indexSpawn;
                }
            }
        }

        spawnLocation ??= DefaultSpawn;

        character.Data.SpawnPositionX = spawnLocation.Position.X + spawnLocation.Scale.X / 2;
        character.Data.SpawnPositionY = spawnLocation.Position.Y + spawnLocation.Scale.Y / 2;

        if (spawnLocation.ParentPlane == "Plane1")
            character.Data.SpawnOnBackPlane = true;
        else if (spawnLocation.ParentPlane == "Plane0")
            character.Data.SpawnOnBackPlane = false;
        else
            Logger.LogWarning("Unknown plane for portal: {PortalPlane}", spawnLocation.ParentPlane);

        Logger.LogDebug(
            "Spawning {CharacterName} at object '{Object}' (spawn '{SpawnPoint}') at '{NewRoom}'.",
            character.Data.CharacterName,
            spawnLocation.Id != 0 ? spawnLocation.Id : "DEFAULT",
            character.LevelData.SpawnPointId != 0 ? character.LevelData.SpawnPointId : "DEFAULT",
            character.LevelData.LevelId
        );

        Logger.LogDebug("Position of spawn: {Position}", spawnLocation.Position);

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

    public void DumpPlayersToLobby()
    {
        foreach (var player in Players.Values)
            player.DumpToLobby();
    }

    public void Dispose(int id)
    {
        var roomEntities = Entities.Values.SelectMany(s => s).ToList();
        foreach (var component in roomEntities)
            if (component.Id == id)
            {
                component.Disposed = true;

                Logger.LogInformation("Disposed component {component} from GameObject {prefabname} with Id {id}",
                    component.GetType().Name, component.PrefabName, component.Id);
            }
    }

    public string GetRoomName() =>
    $"{LevelInfo.LevelId}_{_roomId}";
}
