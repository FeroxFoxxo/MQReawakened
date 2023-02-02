using Server.Base.Network;
using Server.Reawakened.Levels;
using Server.Reawakened.Levels.Enums;
using Server.Reawakened.Levels.Services;

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

        player.JoinLevel(state, newLevel, out var _);
    }

    public static int GetLevelId(this Player player) =>
        player.CurrentLevel != null ? player.CurrentLevel.LevelData.LevelId : -1;

    public static void SendStartPlay(this Player player, int characterId, NetState state, LevelHandler levelHandler)
    {
        player.SetCharacterSelected(characterId);
        var level = levelHandler.GetLevelFromId(player.UserInfo.CharacterLevel[characterId]);
        level?.SendCharacterInfoData(state, player, CharacterInfoType.Detailed);
    }

    public static void SetLevel(this Player player, int levelId, int characterId) =>
        player.UserInfo.CharacterLevel[characterId] = levelId;
}
