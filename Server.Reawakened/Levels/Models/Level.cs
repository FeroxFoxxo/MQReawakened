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
    private readonly ServerConfig _serverConfig;

    public readonly LevelInfo LevelInfo;
    public readonly LevelDataModel LevelData;
    public readonly LevelEntities LevelEntities;

    public readonly long TimeOffset;

    public Level(LevelInfo levelInfo, LevelDataModel levelData, ServerConfig serverConfig, LevelHandler handler)
    {
        _serverConfig = serverConfig;
        _handler = handler;
        _clients = new Dictionary<int, NetState>();
        _clientIds = new HashSet<int>();

        LevelInfo = levelInfo;
        LevelData = levelData;
        TimeOffset = GetTime.GetCurrentUnixMilliseconds();

        LevelEntities = new LevelEntities(levelData);
    }

    public void AddClient(NetState newClient, out JoinReason reason)
    {
        var playerId = -1;

        if (_clientIds.Count > _serverConfig.PlayerCap)
            reason = JoinReason.Full;
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

        switch (reason)
        {
            case JoinReason.Accepted:
                {
                    var newPlayer = newClient.Get<Player>();

                    if (LevelInfo.LevelId == -1)
                        return;

                    // JOIN CONDITION

                    newClient.SendXml("joinOK", $"<pid id='{newPlayer.PlayerId}' /><uLs />");

                    if (LevelInfo.LevelId == 0)
                        return;

                    // WHERE SPAWN

                    var character = newPlayer.GetCurrentCharacter();

                    var spawnPoints = LevelEntities.SpawnPoints;

                    if (character.LastLevel == 0)
                    {
                        var spawn = spawnPoints.MinBy(x => x.Key).Value;

                        character.Data.SpawnPositionX = spawn.Position.X;
                        character.Data.SpawnPositionY = spawn.Position.Y;

                        character.Data.SpawnOnBackPlane = spawn.Position.Z > 1;
                    }
                    else
                    {
                        // TODO: ADD SPAWN FOR WORLD SWITCH
                        throw new MissingMethodException();
                    }

                    // USER ENTER AND CHARACTER DATA

                    var newAccount = newClient.Get<Account>();

                    foreach (var currentClient in _clients.Values)
                    {
                        var currentPlayer = currentClient.Get<Player>();
                        var currentAccount = currentClient.Get<Account>();

                        var areDifferentClients = currentPlayer.UserInfo.UserId != newPlayer.UserInfo.UserId;

                        SendUserEnterData(newClient, currentPlayer, currentAccount);

                        if (areDifferentClients)
                            SendUserEnterData(currentClient, newPlayer, newAccount);

                        SendCharacterInfoData(newClient, currentPlayer,
                            areDifferentClients ? CharacterInfoType.Lite : CharacterInfoType.Portals);

                        if (areDifferentClients)
                            SendCharacterInfoData(currentClient, newPlayer, CharacterInfoType.Lite);
                    }

                    break;
                }
            case JoinReason.Full:
            default:
                newClient.SendXml("joinKO", $"<error>{reason.GetJoinReasonError()}</error>");
                break;
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
