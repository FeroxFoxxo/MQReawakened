using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.System;

public class SystemMailModel
{
    public int SubjectTextId { get; set; }
    public int BodyTextId { get; set; }
    public Dictionary<int, int> ItemIds { get; set; }

    public SystemMailModel() => ItemIds = new Dictionary<int, int>();

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('$');

        sb.Append(SubjectTextId);
        sb.Append(BodyTextId);
        sb.Append(ItemIds.Count);

        foreach (var item in ItemIds)
        {
            sb.Append(item.Key);
            sb.Append(item.Value);
        }

        return sb.ToString();
    }
}
