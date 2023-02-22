using Server.Base.Accounts.Extensions;
using Server.Base.Accounts.Models;
using Server.Base.Network;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Enums;
using Server.Reawakened.Rooms.Services;
using WorldGraphDefines;

namespace Server.Reawakened.Players.Extensions;

public static class RoomExtensions
{
    public static void JoinRoom(this Player player, NetState state, Room room, out JoinReason reason)
    {
        player.CurrentRoom?.RemoveClient(player.UserId);
        player.CurrentRoom = room;
        player.CurrentRoom.AddClient(state, out reason);
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
        player.GetCurrentCharacter()?.LevelData.LevelId ?? -1;

    public static void SendStartPlay(this Player player, CharacterModel character, NetState state, LevelInfo levelInfo)
    {
        character.Data.SetPlayerData(player);
        player.SetCharacterSelected(character.Data.CharacterId);
        state.SendCharacterInfoData(player, CharacterInfoType.Detailed, levelInfo);
    }

    public static void SentEntityTriggered(this NetState netState, int id, bool success, bool active)
    {
        var player = netState.Get<Player>();
        var room = player.CurrentRoom;

        var collectedEvent =
            new Trigger_SyncEvent(id.ToString(), room.Time, success, player.GameObjectId.ToString(), active);

        netState.SendSyncEventToPlayer(collectedEvent);
    }

    public static void SentEntityTriggered(this Room room, int id, Player player, bool success, bool active)
    {
        var collectedEvent =
            new Trigger_SyncEvent(id.ToString(), room.Time, success, player.GameObjectId.ToString(), active);

        room.SendSyncEvent(collectedEvent);
    }

    // Player Id is unused
    public static void SendUserEnterData(this NetState state, Player player, Account account) =>
        state.SendXml("uER",
            $"<u i='{player.UserId}' m='{account.IsModerator()}' s='{account.IsSpectator()}' p='{player.UserId}'>" +
            $"<n>{account.Username}</n>" +
            "</u>"
        );

    public static void SendCharacterInfoData(this NetState state, Player player, CharacterInfoType type,
        LevelInfo levelInfo)
    {
        var character = player.GetCurrentCharacter();

        var info = type switch
        {
            CharacterInfoType.Lite => character.Data.GetLightCharacterData(),
            CharacterInfoType.Portals => character.Data.BuildPortalData(),
            CharacterInfoType.Detailed => character.Data.ToString(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        state.SendXt("ci", player.UserId, info, player.GameObjectId, levelInfo.Name);
    }
}
