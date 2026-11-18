using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Protocol;

public class SentenceModel
{
    public int SentenceTextId { get; set; }

    public int SpecificTextId { get; set; }
    
    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('|');
        
        sb.Append(SentenceTextId);
        sb.Append(SpecificTextId);
        
        return sb.ToString();
    }
}
