using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.XMLs.Models.Npcs;

public class ConversationInfo(int dialogId, int conversationId)
{
    public readonly int DialogId = dialogId;
    public readonly int ConversationId = conversationId;

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('|');

        sb.Append(DialogId);
        sb.Append(ConversationId);

        return sb.ToString();
    }
}
