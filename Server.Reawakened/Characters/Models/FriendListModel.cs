using System.Text;

namespace Server.Reawakened.Characters.Models;

public class FriendListModel
{
    public const char FieldSeparator = '|';

    public Dictionary<string, FriendDataModel> FriendList { get; set; }

    public FriendListModel() {}

    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (var friend in FriendList)
        {
            sb.Append(friend.Value);
            sb.Append(FieldSeparator);
        }

        return sb.ToString();
    }
}
