using Server.Base.Accounts.Extensions;
using Server.Base.Accounts.Models;
using Server.Base.Network;
using Server.Reawakened.Core.Models;
using Server.Reawakened.Core.Network.Extensions;
using Server.Reawakened.Levels.Enums;
using Server.Reawakened.Levels.Extensions;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Players;
using WorldGraphDefines;

namespace Server.Reawakened.Levels;

public class Level
{
    private readonly HashSet<int> _clientIds;
    private readonly Dictionary<int, NetState> _clients;
    private readonly LevelHandler _handler;
    private readonly ServerConfig _serverConfig;

    public readonly LevelInfo LevelData;

    public Level(LevelInfo levelData, ServerConfig serverConfig, LevelHandler handler)
    {
        LevelData = levelData;
        _serverConfig = serverConfig;
        _handler = handler;
        _clients = new Dictionary<int, NetState>();
        _clientIds = new HashSet<int>();
    }

    public void AddClient(NetState state, out JoinReason reason)
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

            _clients.Add(playerId, state);
            _clientIds.Add(playerId);
            reason = JoinReason.Accepted;
        }

        state.Get<Player>().PlayerId = playerId;

        SendClientJoin(state, reason);
    }

    public void SendClientJoin(NetState newClient, JoinReason reason)
    {
        switch (reason)
        {
            case JoinReason.Accepted:
            {
                var newPlayer = newClient.Get<Player>();

                newClient.SendXml("joinOK", $"<pid id='{newPlayer.PlayerId}' /><uLs />");

                if (LevelData.LevelId == -1)
                    return;

                var newAccount = newClient.Get<Account>();

                foreach (var currentClient in _clients.Values)
                {
                    var currentPlayer = currentClient.Get<Player>();
                    var currentAccount = currentClient.Get<Account>();

                    var areDifferentClients = currentPlayer.UserInfo.UserId != newPlayer.UserInfo.UserId;

                    SendUserEnterData(newClient, currentPlayer, currentAccount);

                    if (areDifferentClients)
                        SendUserEnterData(currentClient, newPlayer, newAccount);

                    SendCharacterInfoData(newClient, currentPlayer, areDifferentClients ? CharacterInfoType.Lite : CharacterInfoType.Portals);

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
        state.SendXml("uER", $"<u i='{player.UserInfo.UserId}' m='{account.IsModerator()}' s='{account.IsSpectator()}' p='{player.PlayerId}'><n>{account.Username}</n></u>");

    public void SendCharacterInfoData(NetState state, Player player, CharacterInfoType type)
    {
        var character = player.GetCurrentCharacter();

        var info = type switch
        {
            CharacterInfoType.Lite => character.GetLightCharacterData(),
            CharacterInfoType.Portals => character.BuildPortalData(),
            CharacterInfoType.Detailed => character.ToString(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        state.SendXt("ci", player.UserInfo.UserId.ToString(), info, character.GetGoId().ToString(), LevelData.Name);
    }

    public void DumpPlayersToLobby()
    {
        foreach (var playerId in _clients.Keys)
            DumpPlayerToLobby(playerId);
    }

    public void DumpPlayerToLobby(int playerId)
    {
        var client = _clients[playerId];
        client.Get<Player>().JoinLevel(client, _handler.GetLevelFromId(-1), out var _);
        RemoveClient(playerId);
    }

    public void RemoveClient(int playerId)
    {
        _clients.Remove(playerId);
        _clientIds.Remove(playerId);
    }
}
