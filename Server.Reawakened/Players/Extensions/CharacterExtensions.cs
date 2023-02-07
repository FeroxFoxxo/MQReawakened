using Server.Base.Network;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;
using WorldGraphDefines;
using static LeaderBoardTopScoresJson;

namespace Server.Reawakened.Players.Extensions;

public static class CharacterExtensions
{
    public static CharacterModel GetCurrentCharacter(this Player player)
        => player.UserInfo.Characters[player.CurrentCharacter];

    public static CharacterModel GetCharacterFromName(this Player player, string characterName)
        => player.UserInfo.Characters.Values
            .FirstOrDefault(c => c.Data.CharacterName == characterName);

    public static void SetCharacterSelected(this Player player, int characterId)
    {
        player.CurrentCharacter = characterId;
        player.UserInfo.LastCharacterSelected = player.GetCurrentCharacter().Data.CharacterName;
    }

    public static int GetCharacterObjectId(this CharacterModel characterData) =>
        characterData.Data.UserUuid + characterData.Data.CharacterId;

    public static void AddCharacter(this Player player, CharacterModel characterData) =>
        player.UserInfo.Characters.Add(characterData.Data.CharacterId, characterData);

    public static void DeleteCharacter(this Player player, int id)
    {
        player.UserInfo.Characters.Remove(id);
        
        player.UserInfo.LastCharacterSelected = player.UserInfo.Characters.Count > 0 ?
            player.UserInfo.Characters.First().Value.Data.CharacterName :
            string.Empty;
    }

    public static void LevelUp(this CharacterDataModel characterData, int level)
    {
        characterData.GlobalLevel = level;

        characterData.ReputationForCurrentLevel = GetReputationForLevel(level - 1);
        characterData.ReputationForNextLevel = GetReputationForLevel(level);
        characterData.Reputation = 0;

        characterData.MaxLife = GetHealthForLevel(level);
    }

    private static int GetHealthForLevel(int level) => (level - 1) * 270 + 81;

    private static int GetReputationForLevel(int level) => (Convert.ToInt32(Math.Pow(level, 2)) - (level - 1)) * 500;
    
    public static bool DiscoverTribe(this CharacterModel character, LevelInfo lInfo)
    {
        if (!lInfo.Name.Contains("highway", StringComparison.OrdinalIgnoreCase))
            return false;

        if (character.Data.TribesDiscovered.ContainsKey(lInfo.Tribe))
        {
            if (character.Data.TribesDiscovered[lInfo.Tribe])
                return false;

            character.Data.TribesDiscovered[lInfo.Tribe] = true;
        }
        else
        {
            character.Data.TribesDiscovered.Add(lInfo.Tribe, true);
        }

        return true;
    }

    public static void AddBananas(this Player player, NetState state, int collectedBananas)
    {
        var charData = player.GetCurrentCharacter().Data;
        charData.Cash += collectedBananas;
        state.SendXt("ca", charData.Cash, charData.NCash);
    }

    public static void SendLevelChange(this Player player, NetState netState, LevelHandler levelHandler, WorldGraphXML worldGraph)
    {
        var error = string.Empty;
        var levelName = string.Empty;
        var surroundingLevels = string.Empty;

        try
        {
            var level = player.GetCurrentLevel(levelHandler);

            levelName = level.LevelInfo.Name;
            surroundingLevels = GetSurroundingLevels(level.LevelInfo, worldGraph);
        }
        catch (Exception e)
        {
            error = e.Message;
        }

        netState.SendXt("lw", error, levelName, surroundingLevels);
    }

    private static string GetSurroundingLevels(LevelInfo levelInfo, WorldGraphXML worldGraph)
    {
        var sb = new SeparatedStringBuilder('!');

        var levels = worldGraph.GetLevelWorldGraphNodes(levelInfo.LevelId)
            .Where(x => x.ToLevelID != x.LevelID)
            .Select(x => worldGraph.GetInfoLevel(x.ToLevelID).Name)
            .Distinct();

        foreach (var level in levels)
            sb.Append(level);

        return sb.ToString();
    }
}
