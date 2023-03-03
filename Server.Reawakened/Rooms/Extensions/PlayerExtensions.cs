using Server.Base.Accounts.Extensions;
using Server.Base.Accounts.Models;
using Server.Base.Network;
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
    public static void JoinRoom(this Player player, NetState state, Room room, out JoinReason reason)
    {
        player.Room?.RemoveClient(player);
        player.Room = room;
        player.Room.AddClient(state, out reason);
    }

    public static void QuickJoinRoom(this Player player, int id, NetState state, WorldHandler worldHandler)
    {
        Room newRoom = null;

        try
        {
            newRoom = worldHandler.GetRoomFromLevelId(id);
        }
        catch (NullReferenceException)
        {
        }

        if (newRoom == null)
            return;

        player.JoinRoom(state, newRoom, out _);
    }

    public static int GetLevelId(this Player player) =>
        player.Character?.LevelData.LevelId ?? -1;

    public static void SendStartPlay(this Player player, CharacterModel character, NetState state, LevelInfo levelInfo)
    {
        character.Data.SetPlayerData(player);
        player.SetCharacterSelected(character.Data.CharacterId);
        state.SendCharacterInfoData(player, CharacterInfoType.Detailed, levelInfo);
    }

    public static void SentEntityTriggered(this Room room, int id, Player player, bool success, bool active)
    {
        var collectedEvent =
            new Trigger_SyncEvent(id.ToString(), room.Time, success, player.GameObjectId.ToString(), active);

        room.SendSyncEvent(collectedEvent);
    }

    // PlayerList Id is unused
    public static void SendUserEnterData(this NetState state, Player player, Account account) =>
        state.SendXml("uER",
            $"<u i='{player.UserId}' m='{account.IsModerator()}' s='{account.IsSpectator()}' p='{player.UserId}'>" +
            $"<n>{account.Username}</n>" +
            "</u>"
        );

    public static void SendUserGoneData(this NetState state, Player player) =>
        state.SendXml("userGone",
            $"<user id='{player.UserId}'></user>"
        );

    public static void SendCharacterInfoData(this NetState state, Player player, CharacterInfoType type,
        LevelInfo levelInfo)
    {
        var character = player.Character;

        var info = type switch
        {
            CharacterInfoType.Lite => character.Data.GetLightCharacterData(),
            CharacterInfoType.Portals => character.Data.BuildPortalData(),
            CharacterInfoType.Detailed => character.Data.ToString(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        state.SendXt("ci", player.UserId, info, player.GameObjectId, levelInfo.Name);
    }

    public static void SendLevelUp(this Player player, LevelUpDataModel levelUpData)
    {
        foreach (var client in player.Room.Clients.Values)
            client.SendXt("ce", levelUpData, player.UserId);
    }
}
