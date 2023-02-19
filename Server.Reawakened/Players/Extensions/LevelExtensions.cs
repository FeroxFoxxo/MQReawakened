using Server.Base.Accounts.Extensions;
using Server.Base.Accounts.Models;
using Server.Base.Network;
using Server.Reawakened.Levels;
using Server.Reawakened.Levels.Enums;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Models;
using WorldGraphDefines;

namespace Server.Reawakened.Players.Extensions;

public static class LevelExtensions
{
    public static void JoinLevel(this Player player, NetState state, Level level, out JoinReason reason)
    {
        player.CurrentLevel?.RemoveClient(player.PlayerId);
        player.CurrentLevel = level;
        player.CurrentLevel.AddClient(state, out reason);
    }

    public static void QuickJoinLevel(this Player player, int id, NetState state, LevelHandler levelHandler)
    {
        Level newLevel = null;

        try
        {
            newLevel = levelHandler.GetLevelFromId(id);
        }
        catch (NullReferenceException)
        {
        }

        if (newLevel == null)
            return;

        player.JoinLevel(state, newLevel, out _);
    }

    public static int GetSetLevelId(this Player player) =>
        player.GetCurrentCharacter()?.Level ?? -1;

    public static void SendStartPlay(this Player player, CharacterModel character, NetState state,
        LevelInfo levelInfo, Microsoft.Extensions.Logging.ILogger logger)
    {
        character.SetCharacterSpawn(character.PortalId, 0, logger);
        character.Data.SetPlayerData(player);
        player.SetCharacterSelected(character.Data.CharacterId);
        state.SendCharacterInfoData(player, CharacterInfoType.Detailed, levelInfo);
    }

    public static void SentEntityTriggered(this NetState netState, int id, bool success, bool active)
    {
        var player = netState.Get<Player>();
        var level = player.CurrentLevel;

        var collectedEvent = new Trigger_SyncEvent(id.ToString(), level.Time, success,
            player.PlayerId.ToString(), active);

        netState.SendSyncEventToPlayer(collectedEvent);
    }

    public static void SentEntityTriggered(this Level level, int id, Player player, bool success, bool active)
    {
        var collectedEvent = new Trigger_SyncEvent(id.ToString(), level.Time, success,
            player.PlayerId.ToString(), active);

        level.SendSyncEvent(collectedEvent);
    }

    public static void SendUserEnterData(this NetState state, Player player, Account account) =>
        state.SendXml("uER",
            $"<u i='{player.UserInfo.UserId}' m='{account.IsModerator()}' s='{account.IsSpectator()}' p='{player.PlayerId}'><n>{account.Username}</n></u>");

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

        state.SendXt("ci", player.UserInfo.UserId.ToString(), info, player.PlayerId, levelInfo.Name);
    }
}
