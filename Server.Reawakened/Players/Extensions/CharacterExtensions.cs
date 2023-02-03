using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;

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

    public static void DeleteCharacter(this Player player, int id) =>
        player.UserInfo.Characters.Remove(id);
}
