using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class PlayerListModel
{
    public List<PlayerDataModel> PlayerList { get; set; }

    public PlayerListModel(List<PlayerDataModel> playerList) =>
        PlayerList = playerList;

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('|');

        foreach (var friend in PlayerList)
            sb.Append(friend);

        return sb.ToString();
    }
}
