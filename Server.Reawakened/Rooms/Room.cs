using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Models;
using Server.Base.Core.Extensions;
using Server.Base.Network;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Enums;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.Rooms.Services;
using WorldGraphDefines;
using Timer = Server.Base.Timers.Timer;

namespace Server.Reawakened.Rooms;

public class Room : Timer
{
    private readonly int _roomId;
    private readonly ServerRConfig _config;
    private readonly Level _level;
    private readonly HashSet<int> _gameObjectIds;
    private readonly WorldHandler _worldHandler;

    public readonly Dictionary<int, NetState> Clients;
    public readonly Dictionary<int, List<BaseSyncedEntity>> Entities;
    public readonly ILogger<Room> Logger;

    public readonly Dictionary<string, PlaneModel> Planes;
    public readonly Dictionary<int, List<string>> UnknownEntities;

    public SpawnPointEntity DefaultSpawn { get; set; }

    public LevelInfo LevelInfo => _level.LevelInfo;

    public long TimeOffset { get; set; }
    public long Time => Convert.ToInt64(Math.Floor((GetTime.GetCurrentUnixMilliseconds() - TimeOffset) / 1000.0));

    public Room(
        int roomId, Level level, ServerRConfig config, TimerThread timerThread,
        IServiceProvider services, ILogger<Room> logger, WorldHandler worldHandler
    ) :
        base(TimeSpan.Zero, TimeSpan.FromSeconds(1.0 / config.RoomTickRate), 0, timerThread)
    {
        _roomId = roomId;
        _config = config;
        Logger = logger;
        _worldHandler = worldHandler;
        _level = level;

        Clients = new Dictionary<int, NetState>();
        _gameObjectIds = new HashSet<int>();

        if (LevelInfo.Type == LevelType.Unknown)
            return;

        Planes = LevelInfo.LoadPlanes(_config);
        Entities = this.LoadEntities(services, out UnknownEntities);

        foreach (var gameObjectId in Planes.Values
                     .Select(x => x.GameObjects.Values)
                     .SelectMany(x => x)
                     .Select(x => x.ObjectInfo.ObjectId)
                )
            _gameObjectIds.Add(gameObjectId);

        foreach (var entity in Entities.Values.SelectMany(x => x))
            entity.InitializeEntity();

        var spawnPoints = this.GetEntities<SpawnPointEntity>();

        DefaultSpawn = spawnPoints.Values.MinBy(p => p.Index);

        if (DefaultSpawn == null)
            Logger.LogError("Could not find default spawn for level: {RoomId} ({RoomName})",
                LevelInfo.LevelId, LevelInfo.Name);

        TimeOffset = GetTime.GetCurrentUnixMilliseconds();
        Start();
    }

    public override void OnTick()
    {
        foreach (var entity in Entities.Values.SelectMany(entityList => entityList))
            entity.Update();
    }

    public void AddClient(NetState newClient, out JoinReason reason)
    {
        reason = Clients.Count > _config.PlayerCap ? JoinReason.Full : JoinReason.Accepted;

        if (LevelInfo.LevelId == -1)
            return;

        if (reason == JoinReason.Accepted)
        {
            Clients.Add(newClient.Get<Player>().GameObjectId, newClient);

            newClient.SendXml("joinOK", "<pid id='0' /><uLs />");

            if (LevelInfo.LevelId == 0)
                return;

            JoinRoom(newClient);
        }
        else
        {
            newClient.SendXml("joinKO", $"<error>{reason.GetJoinReasonError()}</error>");
        }
    }

    public void JoinRoom(NetState newClient)
    {
        var newPlayer = newClient.Get<Player>();

        var gameObjectId = 1;

        while (_gameObjectIds.Contains(gameObjectId))
            gameObjectId++;

        _gameObjectIds.Add(gameObjectId);

        newPlayer.GameObjectId = gameObjectId;

        // USER ENTER
        var newAccount = newClient.Get<Account>();

        foreach (var currentClient in Clients.Values)
        {
            var currentPlayer = currentClient.Get<Player>();
            var currentAccount = currentClient.Get<Account>();

            var areDifferentClients = currentPlayer.UserId != newPlayer.UserId;

            newClient.SendUserEnterData(currentPlayer, currentAccount);

            if (areDifferentClients)
                currentClient.SendUserEnterData(newPlayer, newAccount);
        }
    }

    public void SendCharacterInfo(Player player, NetState newClient)
    {
        // WHERE TO SPAWN
        var character = player.Character;

        BaseSyncedEntity spawnLocation = null;

        var spawnPoints = this.GetEntities<SpawnPointEntity>();
        var portals = this.GetEntities<PortalControllerEntity>();

        if (character.LevelData.PortalId != 0)
            if (portals.TryGetValue(character.LevelData.PortalId, out var portal))
                spawnLocation = portal;

        if (spawnLocation == null)
            if (spawnPoints.TryGetValue(character.LevelData.SpawnPointId, out var spawnPoint))
                spawnLocation = spawnPoint;

        if (spawnLocation == null)
        {
            var spawnPoint = spawnPoints.Values.FirstOrDefault(s => s.Index == character.LevelData.SpawnPointId);
            if (spawnPoint != null)
                spawnLocation = spawnPoint;
        }

        spawnLocation ??= DefaultSpawn;

        character.Data.SpawnPositionX = spawnLocation.Position.X + spawnLocation.Scale.X / 2;
        character.Data.SpawnPositionY = spawnLocation.Position.Y + spawnLocation.Scale.Y / 2;
        character.Data.SpawnOnBackPlane = spawnLocation.Position.Z > 1;

        Logger.LogDebug(
            "Spawning {CharacterName} at object '{Object}' (portal '{Portal}' spawn '{SpawnPoint}') at '{NewRoom}'.",
            character.Data.CharacterName,
            spawnLocation.Id != 0 ? spawnLocation.Id : "DEFAULT",
            character.LevelData.PortalId != 0 ? character.LevelData.PortalId : "DEFAULT",
            character.LevelData.SpawnPointId != 0 ? character.LevelData.SpawnPointId : "DEFAULT",
            character.LevelData.LevelId
        );

        Logger.LogDebug("Position of spawn: {Position}", spawnLocation.Position);

        // CHARACTER DATA

        foreach (var currentClient in Clients.Values)
        {
            var currentPlayer = currentClient.Get<Player>();

            var areDifferentClients = currentPlayer.UserId != player.UserId;

            newClient.SendCharacterInfoData(currentPlayer,
                areDifferentClients ? CharacterInfoType.Lite : CharacterInfoType.Portals, LevelInfo);

            if (areDifferentClients)
                currentClient.SendCharacterInfoData(player, CharacterInfoType.Lite, LevelInfo);
        }
    }

    public void DumpPlayersToLobby()
    {
        foreach (var clientId in Clients.Keys)
            DumpPlayerToLobby(clientId);
    }

    public void DumpPlayerToLobby(int clientId)
    {
        var client = Clients[clientId];
        var player = client.Get<Player>();
        player.JoinRoom(client, _worldHandler.GetRoomFromLevelId(-1), out _);
        RemoveClient(player);
    }

    public void RemoveClient(Player player)
    {
        Clients.Remove(player.GameObjectId);
        _gameObjectIds.Remove(player.GameObjectId);

        if (LevelInfo.LevelId <= 0)
            return;

        if (Clients.Count != 0)
        {
            foreach (var client in Clients.Values)
                client.SendUserGoneData(player);

            return;
        }

        _level.Rooms.Remove(_roomId);
        Stop();
    }

    public string GetRoomName() =>
        $"{LevelInfo.LevelId}#{_roomId}";
}
