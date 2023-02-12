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

    public static int GetLevelId(this Player player) =>
        player.CurrentLevel != null ? player.CurrentLevel.LevelInfo.LevelId : -1;

    public static void SendStartPlay(this Player player, CharacterModel character, NetState state,
        LevelHandler levelHandler, Microsoft.Extensions.Logging.ILogger logger)
    {
        character.SetCharacterSpawn(character.PortalId, 0, logger);
        character.Data.SetPlayerData(player);
        player.SetCharacterSelected(character.Data.CharacterId);
        player.SendPlayerData(CharacterInfoType.Detailed, state, levelHandler);
    }

    public static void SendPlayerData(this Player player, CharacterInfoType type, NetState state,
        LevelHandler levelHandler)
    {
        var level = player.GetCurrentLevel(levelHandler);
        state.SendCharacterInfoData(player, type, level?.LevelInfo);
    }

    public static Level GetCurrentLevel(this Player player, LevelHandler levelHandler) =>
        levelHandler.GetLevelFromId(player.GetCurrentCharacter().Level);

    public static void SentEntityTriggered(this Player player, int id, Level level)
    {
        var collectedEvent = new Trigger_SyncEvent(id.ToString(), level.Time, true,
            player.PlayerId.ToString(), true);

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

        state.SendXt("ci", player.UserInfo.UserId.ToString(), info, player.PlayerId,
            levelInfo.Name);
    }
}
