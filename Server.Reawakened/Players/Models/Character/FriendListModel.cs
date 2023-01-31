using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class FriendListModel
{
    public Dictionary<string, FriendDataModel> FriendList { get; set; }

    public FriendListModel() =>
        FriendList = new Dictionary<string, FriendDataModel>();

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('|');

        foreach (var friend in FriendList)
            sb.Append(friend.Value);

        return sb.ToString();
    }
}
