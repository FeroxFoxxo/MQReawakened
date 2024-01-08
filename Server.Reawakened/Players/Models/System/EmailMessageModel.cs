using A2m.Server;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.System;
public class EmailMessageModel
{
    public EmailHeaderModel EmailHeaderModel { get; set; }
    public string Body { get; set; }
    public int BackgroundId { get; set; }
    public int PackageId { get; set; }
    public ItemDescription Item { get; set; }
    public Dictionary<ItemDescription, int> Attachments { get; set; }

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('&');

        sb.Append(EmailHeaderModel);
        sb.Append(Body);
        sb.Append(BackgroundId);
        sb.Append(PackageId);
        sb.Append(Item.ItemId);

        foreach (var kvp in Attachments)
        {
            sb.Append(kvp.Key.ItemId);
            sb.Append(kvp.Value);
        }

        return sb.ToString();
    }
}
