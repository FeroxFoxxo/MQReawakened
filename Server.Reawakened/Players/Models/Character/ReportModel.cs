using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;
public class ReportModel
{
    public Guid Id { get; set; }
    public string Category { get; set; }
    public string Reporter { get; set; }
    public string Reported { get; set; }
    public string Description { get; set; }

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('|');

        sb.Append(Id.ToString());
        sb.Append(Category);
        sb.Append(Reporter);
        sb.Append(Reported);
        sb.Append(Description);

        return sb.ToString();
    }
}
