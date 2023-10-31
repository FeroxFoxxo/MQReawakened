using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Groups;

public class GroupModel
{
    public string LeaderCharacterName { get; set; }
    public List<Player> GroupMembers { get; set; }

    public GroupModel(Player player)
    {
        GroupMembers = [];

        LeaderCharacterName = player.CharacterName;

        GroupMembers.Add(player);
    }

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('#');
        sb.Append(LeaderCharacterName);

        foreach (var player in GroupMembers)
        {
            sb.Append(player.CharacterName);
            sb.Append(player.Room.LevelInfo.LevelId);
            sb.Append(player.Room.LevelInfo.Name);
            sb.Append(player.Room.GetRoomName());
        }

        return sb.ToString();
    }
}
