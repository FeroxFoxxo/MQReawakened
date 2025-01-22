using Server.Base.Accounts.Extensions;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models.Protocol;
using Server.Reawakened.Rooms.Enums;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles.Base;
using WorldGraphDefines;

namespace Server.Reawakened.Rooms.Extensions;

public static class PlayerExtensions
{
    private static void JoinRoom(this Player player, Room room, out JoinReason reason)
    {
        player.Room?.RemoveClient(player);
        player.Room = room;
        player.Room.AddClient(player, out reason);
    }

    public static void QuickJoinRoom(this Player player, int id, WorldHandler worldHandler, out JoinReason reason) =>
        player.JoinRoom(worldHandler.GetRoomFromLevelId(id, player), out reason);

    public static string GetPlayersPlaneString(this Player player)
        => player.TempData.Position.z > 0 ? "Plane1" : "Plane0";

    public static int GetLevelId(this Player player) =>
        player.Character?.LevelId ?? -1;

    public static void SentEntityTriggered(this Room room, string id, Player player, bool success, bool active)
    {
        var collectedEvent =
            new Trigger_SyncEvent(id.ToString(), room.Time, success, player.GameObjectId.ToString(), active);

        room.SendSyncEvent(collectedEvent);
    }

    public static void SendUserEnterDataTo(this Player send, Player receive) =>
        receive.NetState.SendXml("uER",
            $"<u i='{send.UserId}' m='{send.Account.IsModerator()}' s='{send.Account.IsSpectator()}' p='{send.Account.Id}'>" +
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
            CharacterInfoType.Lite => character.GetLightCharacterData(),
            CharacterInfoType.Portals => character.BuildPortalData(),
            CharacterInfoType.Detailed => character.ToString(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        receive.SendXt("ci", send.UserId, info, send.GameObjectId, levelInfo.Name);
    }

    public static void SendLevelUp(this Player player)
    {
        var levelUpData = new LevelUpDataModel
        {
            Level = player.Character.GlobalLevel,
            IncPowerJewel = player.Character.BadgePoints,
        };

        foreach (var currentPlayer in player.Room.GetPlayers())
            currentPlayer.SendXt("ce", levelUpData, player.UserId);

        //Temporary way to earn NC upon level up.
        //(Needed for gameplay improvements as NC is currently unobtainable)
        player.AddNCash(125);

        player.SendSyncEventToPlayer(new Health_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
            player.Character.MaxLife, player.Character.MaxLife, player.GameObjectId.ToString()));
    }

    public static void SendStartPlay(this Player player, CharacterModel character,
        LevelInfo levelInfo, EventPrefabs eventPrefabs)
    {
        character.SetPlayerData(player);
        player.SetCharacterSelected(character);
        player.PlayerContainer.AddPlayer(player);
        player.SendCharacterInfoDataTo(player, CharacterInfoType.Detailed, levelInfo);

        if (eventPrefabs.EventInfo != null)
            player.SendXt("de", eventPrefabs.EventInfo.ToString());

        foreach (var friend in player.PlayerContainer.GetPlayersByFriend(player.CharacterId)
                     .Where(p =>
                         player.Character.Friends
                             .Any(x => x == p.Character.Id)
                     )
                )
            friend.SendXt("fy", player.CharacterName);
    }

    public static void DumpToLobby(this Player player, WorldHandler worldHandler) =>
        player.QuickJoinRoom(-1, worldHandler, out var _);

    public static List<GameObjectModel> GetPlaneEntities(this Player player)
    {
        var planeName = player.TempData.Position.z > 10 ? "Plane1" : "Plane0";
        return [.. player.Room.Planes[planeName].GameObjects.Values.SelectMany(x => x)];
    }

    public static void GroupMemberRoomChanged(this Room room, Player player)
    {
        if (player.TempData.Group == null)
            return;

        foreach (var groupMember in player.TempData.Group.GetMembers())
        {
            groupMember.SendXt(
                "pm",
                player.CharacterName,
                room.LevelInfo.Name,
                room.ToString()
            );
        }
    }

}
