using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class BlockListModel
{
    public Dictionary<string, FriendDataModel> BlockList { get; set; }

    public BlockListModel() =>
        BlockList = new Dictionary<string, FriendDataModel>();

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('|');

        foreach (var block in BlockList)
            sb.Append(block.Value);

        return sb.ToString();
    }
}
