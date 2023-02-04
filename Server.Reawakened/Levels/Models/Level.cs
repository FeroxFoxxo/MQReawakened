using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Extensions;
using Server.Base.Accounts.Models;
using Server.Base.Core.Extensions;
using Server.Base.Network;
using Server.Reawakened.Configs;
using Server.Reawakened.Levels.Enums;
using Server.Reawakened.Levels.Extensions;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Levels.Models.LevelData;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;
using WorldGraphDefines;
using static LeaderBoardTopScoresJson;

namespace Server.Reawakened.Levels.Models;

public class Level
{
    private readonly HashSet<int> _clientIds;
    private readonly Dictionary<int, NetState> _clients;
    private readonly LevelHandler _handler;
    private readonly WorldGraph _worldGraph;
    private readonly ILogger<LevelHandler> _logger;
    private readonly ServerConfig _serverConfig;

    public readonly LevelInfo LevelInfo;
    public readonly LevelDataModel LevelData;
    public readonly LevelEntities LevelEntities;

    public readonly long TimeOffset;

    public Level(LevelInfo levelInfo, LevelDataModel levelData, ServerConfig serverConfig,
        LevelHandler handler, WorldGraph worldGraph, ILogger<LevelHandler> logger)
    {
        _serverConfig = serverConfig;
        _handler = handler;
        _worldGraph = worldGraph;
        _logger = logger;
        _clients = new Dictionary<int, NetState>();
        _clientIds = new HashSet<int>();

        LevelInfo = levelInfo;
        LevelData = levelData;
        TimeOffset = GetTime.GetCurrentUnixMilliseconds();

        LevelEntities = new LevelEntities(this, _serverConfig);
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

            _clients.Add(playerId, newClient);
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

            foreach (var currentClient in _clients.Values)
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

        var nodes = _worldGraph.GetLevelWorldGraphNodes(character.LastLevel);

        if (nodes != null)
            node = nodes.FirstOrDefault(a => a.ToLevelID == character.Level);

        ObjectInfoModel spawn = null;

        if (node != null)
        {
            _logger.LogDebug("Node Found: Portal ID '{Portal}', Spawn ID '{Spawn}'.", node.PortalID, node.ToSpawnID);
            var portal = LevelEntities.Portals.Values.FirstOrDefault(a => a.ObjectId == node.PortalID);
            var spawnPoint = LevelEntities.SpawnPoints.Values.FirstOrDefault(a => a.ObjectId == node.ToSpawnID);

            if (portal == null)
                if (spawnPoint == null)
                    _logger.LogError("Could not find portal '{PortalId}' or spawn '{SpawnId}'.", node.PortalID, node.ToSpawnID);
                else
                    spawn = spawnPoint;
            else
                spawn = portal;
        }
        else
        {
            _logger.LogError("Could not find node for '{Old}' -> '{New}'.", character.LastLevel, character.Level);
        }

        spawn ??= LevelEntities.SpawnPoints.First().Value;

        character.Data.SpawnPositionX = spawn.Position.X;
        character.Data.SpawnPositionY = spawn.Position.Y;
        character.Data.SpawnOnBackPlane = spawn.Position.Z > 1;

        _logger.LogDebug("Spawning {CharacterName} at object '{NodePortalId}', from '{OldLevel}' to '{NewLevel}'.",
            character.Data.CharacterName, node != null ? node.PortalID : "DEFAULT", character.LastLevel, character.Level);

        _logger.LogDebug("Position of spawn: {Position}", spawn.Position);

        // CHARACTER DATA

        foreach (var currentClient in _clients.Values)
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
        foreach (var playerId in _clients.Keys)
            DumpPlayerToLobby(playerId);
    }

    public void DumpPlayerToLobby(int playerId)
    {
        var client = _clients[playerId];
        client.Get<Player>().JoinLevel(client, _handler.GetLevelFromId(-1), out _);
        RemoveClient(playerId);
    }

    public void RemoveClient(int playerId)
    {
        _clients.Remove(playerId);
        _clientIds.Remove(playerId);
    }
}
