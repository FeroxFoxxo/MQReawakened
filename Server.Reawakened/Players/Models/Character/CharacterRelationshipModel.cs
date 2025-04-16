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
        var otherCharacter = currentPlayer.CharacterHandler.GetCharacterFromId(characterId);

        var otherPlayer = currentPlayer.PlayerContainer.GetPlayersByCharacterId(characterId)
            .FirstOrDefault(p => p.Character.Id == characterId);

        CharacterName = otherCharacter != null ? otherCharacter.CharacterName : "unknown";
        CharacterId = characterId;

        IsOnline = otherPlayer != null;

        Level = otherCharacter != null ? otherCharacter.GlobalLevel : 1;
        Location = otherPlayer != null ? otherPlayer.Room.ToString() : "unknown";

        IsBlocked = currentPlayer.Character.Blocked.Any(x => x == characterId);
        IsMuted = currentPlayer.Character.Muted.Any(x => x == characterId);

        InteractionStatus = otherCharacter != null ? (int)otherCharacter.InteractionStatus : 0;

        if (otherCharacter == null && CharacterName == "unknown")
            currentPlayer.Character.Write.Friends.Remove(characterId);
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
