using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Models;
using Server.Base.Core.Extensions;
using Server.Base.Network;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities;
using Server.Reawakened.Rooms.Enums;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Helpers;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using WorldGraphDefines;
using Timer = Server.Base.Timers.Timer;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Rooms;

public class Room : Timer
{
    private readonly HashSet<int> _gameObjectIds;
    private readonly WorldHandler _worldHandler;
    private readonly ServerStaticConfig _config;

    public readonly Dictionary<int, NetState> Clients;

    public readonly Dictionary<string, PlaneModel> Planes;
    public readonly Dictionary<int, List<BaseSyncedEntity>> Entities;
    public readonly Dictionary<int, List<string>> UnknownEntities;
    public readonly ILogger<Room> Logger;
    public readonly LevelInfo LevelInfo;

    public SpawnPointEntity DefaultSpawn { get; set; }

    public long TimeOffset { get; set; }
    public long Time => Convert.ToInt64(Math.Floor((GetTime.GetCurrentUnixMilliseconds() - TimeOffset) / 1000.0));

    public Room(LevelInfo levelInfo, ServerStaticConfig config,
        WorldHandler worldHandler, ReflectionUtils reflection, TimerThread timerThread,
        IServiceProvider services, ILogger<Room> logger) :
        base(TimeSpan.Zero, TimeSpan.FromSeconds(1.0 / config.RoomTickRate), 0, timerThread)
    {
        _config = config;
        _worldHandler = worldHandler;
        Logger = logger;
        LevelInfo = levelInfo;

        Clients = new Dictionary<int, NetState>();
        _gameObjectIds = new HashSet<int>();

        if (levelInfo.Type == LevelType.Unknown)
            return;

        Planes = LevelInfo.LoadPlanes(_config);
        Entities = this.LoadEntities(reflection, services, out UnknownEntities);

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
                levelInfo.LevelId, levelInfo.Name);

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
        var playerId = -1;

        if (Clients.Count > _config.PlayerCap)
        {
            reason = JoinReason.Full;
        }
        else
        {
            playerId = 1;

            while (_gameObjectIds.Contains(playerId))
                playerId++;

            Clients.Add(playerId, newClient);
            _gameObjectIds.Add(playerId);
            reason = JoinReason.Accepted;
        }

        newClient.Get<Player>().PlayerId = playerId;

        if (reason == JoinReason.Accepted)
        {
            var newPlayer = newClient.Get<Player>();

            if (LevelInfo.LevelId == -1)
                return;

            // JOIN CONDITION
            newClient.SendXml("joinOK", $"<pid id='{newPlayer.PlayerId}' /><uLs />");

            if (LevelInfo.LevelId == 0)
                return;

            // USER ENTER
            var newAccount = newClient.Get<Account>();

            foreach (var currentClient in Clients.Values)
            {
                var currentPlayer = currentClient.Get<Player>();
                var currentAccount = currentClient.Get<Account>();

                var areDifferentClients = currentPlayer.UserInfo.UserId != newPlayer.UserInfo.UserId;

                newClient.SendUserEnterData(currentPlayer, currentAccount);

                if (areDifferentClients)
                    currentClient.SendUserEnterData(newPlayer, newAccount);
            }
        }
        else
        {
            newClient.SendXml("joinKO", $"<error>{reason.GetJoinReasonError()}</error>");
        }
    }

    public void SendCharacterInfo(Player newPlayer, NetState newClient)
    {
        // WHERE TO SPAWN
        var character = newPlayer.GetCurrentCharacter();

        BaseSyncedEntity spawnLocation = null;

        var spawnPoints = this.GetEntities<SpawnPointEntity>();
        var portals = this.GetEntities<PortalControllerEntity>();

        if (character.LevelInfo.PortalId != 0)
            if (portals.TryGetValue(character.LevelInfo.PortalId, out var portal))
                spawnLocation = portal;

        if (spawnLocation == null)
            if (spawnPoints.TryGetValue(character.LevelInfo.SpawnPointId, out var spawnPoint))
                spawnLocation = spawnPoint;

        if (spawnLocation == null)
        {
            var spawnPoint = spawnPoints.Values.FirstOrDefault(s => s.Index == character.LevelInfo.SpawnPointId);
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
            character.LevelInfo.PortalId != 0 ? character.LevelInfo.PortalId : "DEFAULT",
            character.LevelInfo.SpawnPointId != 0 ? character.LevelInfo.SpawnPointId : "DEFAULT",
            character.LevelInfo.LevelId
        );

        Logger.LogDebug("Position of spawn: {Position}", spawnLocation.Position);

        // CHARACTER DATA

        foreach (var currentClient in Clients.Values)
        {
            var currentPlayer = currentClient.Get<Player>();

            var areDifferentClients = currentPlayer.UserInfo.UserId != newPlayer.UserInfo.UserId;

            newClient.SendCharacterInfoData(currentPlayer,
                areDifferentClients ? CharacterInfoType.Lite : CharacterInfoType.Portals, LevelInfo);

            if (areDifferentClients)
                currentClient.SendCharacterInfoData(newPlayer, CharacterInfoType.Lite, LevelInfo);
        }
    }

    public void DumpPlayersToLobby()
    {
        foreach (var playerId in Clients.Keys)
            DumpPlayerToLobby(playerId);
    }

    public void DumpPlayerToLobby(int playerId)
    {
        var client = Clients[playerId];
        client.Get<Player>().JoinRoom(client, _worldHandler.GetRoomFromLevelId(-1), out _);
        RemoveClient(playerId);
    }

    public void RemoveClient(int playerId)
    {
        Clients.Remove(playerId);
        _gameObjectIds.Remove(playerId);

        if (Clients.Count != 0 || LevelInfo.LevelId <= 0)
            return;

        _worldHandler.RemoveRoom(this);
        Stop();
    }

    public void SendSyncEvent(SyncEvent syncEvent, Player sentPlayer = null)
    {
        foreach (
            var client in
            from client in Clients.Values
            let receivedPlayer = client.Get<Player>()
            where sentPlayer == null || receivedPlayer.UserInfo.UserId != sentPlayer.UserInfo.UserId
            select client
        )
            client.SendSyncEventToPlayer(syncEvent);
    }
}
