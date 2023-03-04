using Server.Base.Network;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Groups;

public class GroupModel
{
    public string LeaderCharacterName { get; set; }
    public List<NetState> GroupMembers { get; set; }

    public GroupModel(NetState netState)
    {
        GroupMembers = new List<NetState>();

        var player = netState.Get<Player>();
        LeaderCharacterName = player.Character.Data.CharacterName;

        GroupMembers.Add(netState);
    }

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('#');
        sb.Append(LeaderCharacterName);

        foreach (var player in GroupMembers.Select(member => member.Get<Player>()))
        {
            sb.Append(player.Character.Data.CharacterName);
            sb.Append(player.Room.LevelInfo.LevelId);
            sb.Append(player.Room.LevelInfo.Name);
            sb.Append(player.Room.GetRoomName());
        }

        return sb.ToString();
    }
}
