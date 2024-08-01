using Server.Reawakened.Core.Services;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Character;
using System.Text;

namespace Protocols.External._o__ReportMessageHandler;
public class ReportAbuse : ExternalProtocol
{
    public override string ProtocolName => "oa";

    public DiscordHandler DiscordHandler { get; set; }
    public PlayerContainer PlayerContainer { get; set; }

    public override void Run(string[] message)
    {
        var category = Encoding.UTF8.GetString(Convert.FromBase64String(message[5].Replace("category$", "")));
        var character = Encoding.UTF8.GetString(Convert.FromBase64String(message[6].Replace("character$", "")));
        var reportMessage = Encoding.UTF8.GetString(Convert.FromBase64String(message[7].Replace("message$", "")));

        var reportId = Guid.NewGuid();

        DiscordHandler.SendAbuseReport(reportId.ToString(), category, Player.CharacterName, character, reportMessage);

        PlayerContainer.GetPlayerByName(character)?.Character.Reports.TryAdd(reportId.ToString(), new ReportModel
        {
            Id = reportId,
            Category = category,
            Reporter = Player.CharacterName,
            Reported = character,
            Description = reportMessage
        });

        SendXt("oa", "1");
    }
}
