using Server.Base.Accounts.Extensions;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Protocol;
using Server.Reawakened.Rooms.Enums;
using Server.Reawakened.Rooms.Services;
using WorldGraphDefines;

namespace Server.Reawakened.Rooms.Extensions;

public static class PlayerExtensions
{
    public static void JoinRoom(this Player player, Room room, out JoinReason reason)
    {
        player.Room?.RemoveClient(player);
        player.Room = room;
        player.Room.AddClient(player, out reason);
    }

    public static void QuickJoinRoom(this Player player, int id, WorldHandler worldHandler)
    {
        var room = worldHandler.GetRoomFromLevelId(id, player);
        player.JoinRoom(room, out _);
    }

    public static int GetLevelId(this Player player) =>
        player.Character?.LevelData.LevelId ?? -1;

    public static void SentEntityTriggered(this Room room, int id, Player player, bool success, bool active)
    {
        var collectedEvent =
            new Trigger_SyncEvent(id.ToString(), room.Time, success, player.GameObjectId.ToString(), active);

        room.SendSyncEvent(collectedEvent);
    }

    public static void SendUserEnterDataTo(this Player send, Player receive) =>
        receive.NetState.SendXml("uER",
            $"<u i='{send.UserId}' m='{send.Account.IsModerator()}' s='{send.Account.IsSpectator()}' p='{send.Account.UserId}'>" +
            $"<n>{send.Account.Username}</n>" +
            "</u>"
        );

    public static void SendUserGoneDataTo(this Player send, Player receive) =>
        receive.NetState.SendXml("userGone",
            $"<user id='{send.UserId}'></user>"
        );

    public static void SendCharacterInfoDataTo(this Player send, Player receive, CharacterInfoType type,
        LevelInfo levelInfo)
    {
        var character = send.Character;

        var info = type switch
        {
            CharacterInfoType.Lite => character.Data.GetLightCharacterData(),
            CharacterInfoType.Portals => character.Data.BuildPortalData(),
            CharacterInfoType.Detailed => character.Data.ToString(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        receive.SendXt("ci", send.UserId, info, send.GameObjectId, levelInfo.Name);
    }

    public static void SendLevelUp(this Player player)
    {
        var levelUpData = new LevelUpDataModel
        {
            Level = player.Character.Data.GlobalLevel
        };

        foreach (var currentPlayer in player.Room.Players.Values)
            currentPlayer.SendXt("ce", levelUpData, player.UserId);
    }

    public static void SendStartPlay(this Player player, CharacterModel character, LevelInfo levelInfo)
    {
        character.Data.SetPlayerData(player);
        player.SetCharacterSelected(character.Data.CharacterId);
        player.PlayerHandler.AddPlayer(player);
        player.SendCharacterInfoDataTo(player, CharacterInfoType.Detailed, levelInfo);

        foreach (var friend in player.PlayerHandler.GetPlayersByFriend(player.UserId)
                     .Where(p =>
                         player.Character.Data.FriendList
                             .Any(x => x.Key == p.UserId && x.Value == p.Character.Data.CharacterId)
                     )
                )
            friend.SendXt("fy", player.CharacterName);
    }

    public static void DumpToLobby(this Player player)
    {
        var room = player.PlayerHandler.WorldHandler.GetRoomFromLevelId(-1, player);
        player.JoinRoom(room, out _);
    }
}
