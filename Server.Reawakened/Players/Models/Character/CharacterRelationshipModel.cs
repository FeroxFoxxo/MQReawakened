using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class CharacterRelationshipModel
{
    public string CharacterName { get; set; }
    public int CharacterId { get; set; }
    public bool IsOnline { get; set; }
    public int Level { get; set; }
    public string Location { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsMuted { get; set; }
    public int InteractionStatus { get; set; }

    public CharacterRelationshipModel(int characterId, Player currentPlayer)
    {
        var otherCharacter = currentPlayer.CharacterHandler.Get(characterId);
        var otherPlayer = currentPlayer.PlayerContainer.GetPlayersByCharacterId(characterId)
            .FirstOrDefault(p => p.Character.Id == characterId);

        CharacterName = otherCharacter.Data.CharacterName;
        CharacterId = characterId;

        IsOnline = otherPlayer != null;

        Level = otherCharacter.LevelData.LevelId;
        Location = otherPlayer != null ? otherPlayer.Room.ToString() : "unknown";

        IsBlocked = currentPlayer.Character.Data.Blocked.Any(x => x == characterId);
        IsMuted = currentPlayer.Character.Data.Muted.Any(x => x == characterId);

        InteractionStatus = (int)otherCharacter.Data.InteractionStatus;
    }

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('>');

        sb.Append(CharacterId);
        sb.Append(CharacterName);
        sb.Append(IsOnline ? 1 : 0);
        sb.Append(Level);
        sb.Append(Location);
        sb.Append(IsBlocked ? 1 : 0);
        sb.Append(IsMuted ? 1 : 0);
        sb.Append(InteractionStatus);

        return sb.ToString();
    }
}
