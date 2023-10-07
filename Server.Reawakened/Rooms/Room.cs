using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Models;
using Server.Base.Core.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities;
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
    private readonly HashSet<int> _gameObjectIds;

    public readonly Dictionary<int, Player> Players;
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
        IServiceProvider services, ILogger<Room> logger
    ) : base(TimeSpan.Zero, TimeSpan.FromSeconds(1.0 / config.RoomTickRate), 0, timerThread)
    {
        _roomId = roomId;
        _config = config;
        Logger = logger;
        _level = level;

        Players = new Dictionary<int, Player>();
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

        foreach (var player in Players.Values.Where(
                     player => GetTime.GetCurrentUnixMilliseconds() - player.CurrentPing > _config.KickAfterTime
                 ))
            player.Remove(Logger);
    }

    public void GroupMemberRoomChanged(Player player)
    {
        if (player.Group == null)
            return;

        foreach (var groupMember in player.Group.GroupMembers)
        {
            groupMember.SendXt(
                "pm",
                player.Character.Data.CharacterName,
                LevelInfo.Name,
                GetRoomName()
            );
        }
    }

    public void AddClient(Player newPlayer, out JoinReason reason)
    {
        reason = Players.Count > _config.PlayerCap ? JoinReason.Full : JoinReason.Accepted;

        if (LevelInfo.LevelId == -1)
            return;

        if (reason == JoinReason.Accepted)
        {
            var gameObjectId = 1;

            while (_gameObjectIds.Contains(gameObjectId))
                gameObjectId++;

            _gameObjectIds.Add(gameObjectId);

            newPlayer.GameObjectId = gameObjectId;

            Players.Add(gameObjectId, newPlayer);

            GroupMemberRoomChanged(newPlayer);

            newPlayer.NetState.SendXml("joinOK", $"<pid id='{gameObjectId}' /><uLs />");

            if (LevelInfo.LevelId == 0)
                return;

            // USER ENTER
            var newAccount = newPlayer.NetState.Get<Account>();

            foreach (var currentPlayer in Players.Values)
            {
                var currentAccount = currentPlayer.NetState.Get<Account>();

                var areDifferentClients = currentPlayer.UserId != newPlayer.UserId;

                newPlayer.SendUserEnterDataTo(currentPlayer, currentAccount);

                if (areDifferentClients)
                    currentPlayer.SendUserEnterDataTo(newPlayer, newAccount);
            }
        }
        else
        {
            newPlayer.NetState.SendXml("joinKO", $"<error>{reason.GetJoinReasonError()}</error>");
        }
    }

    public void RemoveClient(Player player)
    {
        Players.Remove(player.GameObjectId);
        _gameObjectIds.Remove(player.GameObjectId);

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
    
    public string GetRoomName() =>
        $"{LevelInfo.LevelId}_{_roomId}";
}
