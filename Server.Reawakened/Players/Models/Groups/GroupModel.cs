using Server.Base.Network;

namespace Server.Reawakened.Players.Models.Groups;

public class GroupModel
{
    public string LeaderCharacter { get; set; }
    public List<NetState> GroupMembers { get; set; }

    public GroupModel(NetState netState)
    {
        GroupMembers = new List<NetState>();

        var player = netState.Get<Player>();
        LeaderCharacter = player.Character.Data.CharacterName;

        GroupMembers.Add(netState);
    }
}
