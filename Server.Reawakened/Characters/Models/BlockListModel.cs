using Server.Reawakened.Characters.Helpers;

namespace Server.Reawakened.Characters.Models;

public class BlockListModel
{
    public const char FieldSeparator = '|';

    public Dictionary<string, FriendDataModel> BlockList { get; set; }

    public BlockListModel() =>
        BlockList = new Dictionary<string, FriendDataModel>();

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder(FieldSeparator);

        foreach (var block in BlockList)
            sb.Append(block.Value);

        return sb.ToString();
    }
}
