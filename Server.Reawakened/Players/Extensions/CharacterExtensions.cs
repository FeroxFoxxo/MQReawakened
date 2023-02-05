using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;
using WorldGraphDefines;

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
}
