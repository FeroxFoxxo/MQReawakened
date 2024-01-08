using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class PlayerListModel(List<CharacterRelationshipModel> playerList)
{
    public List<CharacterRelationshipModel> PlayerList { get; set; } = playerList;

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('|');

        foreach (var friend in PlayerList)
            sb.Append(friend);

        return sb.ToString();
    }
}
