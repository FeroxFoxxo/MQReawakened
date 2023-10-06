using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class PlayerDataModel
{
    public string CharacterName { get; set; }
    public int CharacterId { get; set; }
    public bool IsOnline { get; set; }
    public int Level { get; set; }
    public string Location { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsMuted { get; set; }
    public int InteractionStatus { get; set; }

    public PlayerDataModel(int userId, int characterId, Player currentPlayer)
    {
        var user = currentPlayer.PlayerHandler.UserInfoHandler.Data[userId];
        var character = user.Characters[characterId];
        var player = currentPlayer.PlayerHandler.PlayerList
            .Where(p => p.UserId == userId)
            .FirstOrDefault(p => p.Character.Data.CharacterId == characterId);

        CharacterName = character.Data.CharacterName;
        CharacterId = characterId;

        IsOnline = player != null;

        Level = character.LevelData.LevelId;
        Location = player != null ? player.Room.GetRoomName() : "UNKNOWN";

        IsBlocked = currentPlayer.Character.Data.BlockedList.Any(x => x.Key == userId && x.Value == userId);
        IsMuted = currentPlayer.Character.Data.MutedList.Any(x => x.Key == userId && x.Value == userId);

        InteractionStatus = (int)character.Data.InteractionStatus;
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
