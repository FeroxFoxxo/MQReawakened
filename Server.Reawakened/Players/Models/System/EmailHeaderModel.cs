using A2m.Server;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.System;

public class EmailHeaderModel
{
    public int MessageId { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public string Subject { get; set; }
    public string SentTime { get; set; }
    public EmailHeader.EmailStatus Status { get; set; }
    public EmailCategory CategoryId { get; set; }

    public EmailHeaderModel() { }

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('!');

        sb.Append(MessageId);
        sb.Append(From);
        sb.Append(To);
        sb.Append(Subject);
        sb.Append(SentTime);
        sb.Append((int)Status);
        sb.Append((int)CategoryId);

        return sb.ToString();
    }
}
