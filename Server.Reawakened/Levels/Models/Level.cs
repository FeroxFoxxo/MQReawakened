using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Extensions;
using Server.Base.Accounts.Models;
using Server.Base.Core.Extensions;
using Server.Base.Network;
using Server.Reawakened.Configs;
using Server.Reawakened.Levels.Enums;
using Server.Reawakened.Levels.Extensions;
using Server.Reawakened.Levels.Models.LevelData;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Levels.SyncedData;
using Server.Reawakened.Levels.SyncedData.Entities;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;
using WorldGraphDefines;

namespace Server.Reawakened.Levels.Models;

public class Level
{
    private readonly WorldGraph _worldGraph;
    private readonly ILogger<LevelHandler> _logger;
    private readonly ServerConfig _serverConfig;

    private readonly HashSet<int> _clientIds;

    public readonly Dictionary<int, NetState> Clients;
    public readonly LevelHandler LevelHandler;

    public LevelInfo LevelInfo { get; set; }
    public LevelPlanes LevelPlaneHandler { get; set; }
    public LevelEntities LevelEntityHandler { get; set; }

    public long TimeOffset { get; set; }
    
    public long Time => Convert.ToInt64(Math.Floor((GetTime.GetCurrentUnixMilliseconds() - TimeOffset) / 1000.0));

    public Level(LevelInfo levelInfo, LevelPlanes levelPlaneHandler, ServerConfig serverConfig,
        LevelHandler levelHandler, WorldGraph worldGraph, ILogger<LevelHandler> logger)
    {
        _serverConfig = serverConfig;
        LevelHandler = levelHandler;
        _worldGraph = worldGraph;
        _logger = logger;
        Clients = new Dictionary<int, NetState>();
        _clientIds = new HashSet<int>();

        LevelInfo = levelInfo;
        LevelPlaneHandler = levelPlaneHandler;
        TimeOffset = GetTime.GetCurrentUnixMilliseconds();

        LevelEntityHandler = new LevelEntities(this, _logger);
    }

    public void AddClient(NetState newClient, out JoinReason reason)
    {
        var playerId = -1;

        if (_clientIds.Count > _serverConfig.PlayerCap)
        {
            reason = JoinReason.Full;
        }
        else
        {
            playerId = 1;

            while (_clientIds.Contains(playerId))
                playerId++;

            Clients.Add(playerId, newClient);
            _clientIds.Add(playerId);
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

                SendUserEnterData(newClient, currentPlayer, currentAccount);

                if (areDifferentClients)
                    SendUserEnterData(currentClient, newPlayer, newAccount);
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

        DestNode node = null;
        Vector3Model spawnLocation = null;

        var spawnPoints = LevelEntityHandler.GetEntities<SpawnPointEntity>();
        var portals = LevelEntityHandler.GetEntities<PortalControllerEntity>();

        if (character.LastLevel != 0)
        {
            var nodes = _worldGraph.GetLevelWorldGraphNodes(character.LastLevel);

            if (nodes != null)
                node = nodes.FirstOrDefault(a => a.ToLevelID == character.Level);

            if (node != null)
            {
                _logger.LogDebug("Node Found: Portal ID '{Portal}', Spawn ID '{Spawn}'.", node.PortalID,
                    node.ToSpawnID);

                if (portals.TryGetValue(node.PortalID, out var portal))
                {
                    spawnLocation = portal.StoredEntity.Position;
                }
                else
                {
                    if (spawnPoints.TryGetValue(node.PortalID, out var spawnPoint))
                        spawnLocation = spawnPoint.StoredEntity.Position;
                    else
                        _logger.LogError("Could not find portal '{PortalId}' or spawn '{SpawnId}'.", node.PortalID,
                            node.ToSpawnID);
                }
            }
            else
            {
                _logger.LogError("Could not find node for '{Old}' -> '{New}'.", character.LastLevel, character.Level);
            }
        }

        var defaultSpawn = spawnPoints.Values.MinBy(p => p.Index);

        if (defaultSpawn != null)
        {
            spawnLocation ??= defaultSpawn.StoredEntity.Position;
        }
        else
        {
            _logger.LogError("Could not find default spawn point in {LevelId}, as there are none initialized!",
                LevelInfo.LevelId);
            spawnLocation = new Vector3Model();
        }

        character.Data.SpawnPositionX = spawnLocation.X;
        character.Data.SpawnPositionY = spawnLocation.Y;
        character.Data.SpawnOnBackPlane = spawnLocation.Z > 1;

        _logger.LogDebug("Spawning {CharacterName} at object '{NodePortalId}', from '{OldLevel}' to '{NewLevel}'.",
            character.Data.CharacterName, node != null ? node.PortalID : "DEFAULT", character.LastLevel,
            character.Level);

        _logger.LogDebug("Position of spawn: {Position}", spawnLocation);

        // CHARACTER DATA

        foreach (var currentClient in Clients.Values)
        {
            var currentPlayer = currentClient.Get<Player>();

            var areDifferentClients = currentPlayer.UserInfo.UserId != newPlayer.UserInfo.UserId;

            SendCharacterInfoData(newClient, currentPlayer,
                areDifferentClients ? CharacterInfoType.Lite : CharacterInfoType.Portals);

            if (areDifferentClients)
                SendCharacterInfoData(currentClient, newPlayer, CharacterInfoType.Lite);
        }
    }

    private static void SendUserEnterData(NetState state, Player player, Account account) =>
        state.SendXml("uER",
            $"<u i='{player.UserInfo.UserId}' m='{account.IsModerator()}' s='{account.IsSpectator()}' p='{player.PlayerId}'><n>{account.Username}</n></u>");

    public void SendCharacterInfoData(NetState state, Player player, CharacterInfoType type)
    {
        var character = player.GetCurrentCharacter();

        var info = type switch
        {
            CharacterInfoType.Lite => character.Data.GetLightCharacterData(),
            CharacterInfoType.Portals => character.Data.BuildPortalData(),
            CharacterInfoType.Detailed => character.Data.ToString(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        state.SendXt("ci", player.UserInfo.UserId.ToString(), info, character.Data.GetGoId().ToString(),
            LevelInfo.Name);
    }

    public void DumpPlayersToLobby()
    {
        foreach (var playerId in Clients.Keys)
            DumpPlayerToLobby(playerId);
    }

    public void DumpPlayerToLobby(int playerId)
    {
        var client = Clients[playerId];
        client.Get<Player>().JoinLevel(client, LevelHandler.GetLevelFromId(-1), out _);
        RemoveClient(playerId);
    }

    public void RemoveClient(int playerId)
    {
        Clients.Remove(playerId);
        _clientIds.Remove(playerId);

        if (Clients.Count == 0 && LevelInfo.LevelId > 0)
            LevelHandler.RemoveLevel(LevelInfo.LevelId);
    }

    public void SendSyncEvent(SyncEvent syncEvent, Player sentPlayer = null)
    {
        var syncEventMsg = syncEvent.EncodeData();

        foreach (
            var client in
            from client in Clients.Values
            let receivedPlayer = client.Get<Player>()
            where sentPlayer == null || receivedPlayer.UserInfo.UserId != sentPlayer.UserInfo.UserId
            select client
        )
            client.SendXt("ss", syncEventMsg);
    }
}
