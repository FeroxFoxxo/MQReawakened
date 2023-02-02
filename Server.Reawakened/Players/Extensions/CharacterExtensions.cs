using Server.Reawakened.Players.Models.Character;

namespace Server.Reawakened.Players.Extensions;
public static class CharacterExtensions
{

    public static CharacterDataModel GetCurrentCharacter(this Player player)
        => player.UserInfo.Characters[player.CurrentCharacter];

    public static CharacterDataModel GetCharacterFromName(this Player player, string characterName)
        => player.UserInfo.Characters.Values
            .FirstOrDefault(c => c.CharacterName == characterName);

    public static void SetCharacterSelected(this Player player, int characterId)
    {
        player.CurrentCharacter = characterId;
        player.UserInfo.LastCharacterSelected = player.GetCurrentCharacter().CharacterName;
    }

    public static void AddCharacter(this Player player, CharacterDataModel characterData) =>
        player.UserInfo.Characters.Add(characterData.CharacterId, characterData);

    public static void DeleteCharacter(this Player player, int id)
    {
        player.UserInfo.Characters.Remove(id);
        player.UserInfo.CharacterLevel.Remove(id);
    }
}
