namespace Server.Reawakened.Players.Models.Character;
public class ReportModel
{
    public Guid Id { get; set; }
    public string Category { get; set; }
    public string Reporter { get; set; }
    public string Reported { get; set; }
    public string Description { get; set; }
}
