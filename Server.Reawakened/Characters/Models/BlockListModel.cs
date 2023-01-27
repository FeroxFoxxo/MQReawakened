using System.Text;

namespace Server.Reawakened.Characters.Models;

public class BlockListModel
{
    public const char FieldSeparator = '|';

    public Dictionary<string, FriendDataModel> BlockList { get; set; }

    public BlockListModel() =>
        BlockList = new Dictionary<string, FriendDataModel>();

    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (var block in BlockList)
        {
            sb.Append(block.Value);
            sb.Append(FieldSeparator);
        }

        return sb.ToString();
    }
}
