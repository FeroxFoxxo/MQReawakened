using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Groups;

public class GroupModel
{
    private object Lock { get; set; } = new object();

    private string _leaderCharacterName;
    private readonly List<Player> _groupMembers;

    public GroupModel(Player player)
    {
        _groupMembers = [];
        _leaderCharacterName = player.CharacterName;
        _groupMembers.Add(player);
    }

    public void SetLeaderName(string newLeader)
    {
        lock (Lock)
        {
            if (_groupMembers.Any(c => c.CharacterName == newLeader))
                _leaderCharacterName = newLeader;
        }
    }

    public string GetLeaderName()
    {
        lock (Lock)
            return _leaderCharacterName;
    }

    public void AddPlayer(Player player)
    {
        lock (Lock)
        {
            if (_groupMembers.Any(c => c == player))
                return;

            _groupMembers.Add(player);
        }
    }

    public void RemovePlayer(Player player)
    {
        lock (Lock)
        {
            if (!_groupMembers.Any(c => c == player))
                return;

            _groupMembers.Remove(player);
        }
    }

    public List<Player> GetMembers()
    {
        lock (Lock)
            return [.. _groupMembers];
    }

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('#');
        sb.Append(GetLeaderName());

        foreach (var player in GetMembers())
        {
            sb.Append(player.CharacterName);
            sb.Append(player.Room.LevelInfo.LevelId);
            sb.Append(player.Room.LevelInfo.Name);
            sb.Append(player.Room.GetRoomName());
        }

        return sb.ToString();
    }
}
