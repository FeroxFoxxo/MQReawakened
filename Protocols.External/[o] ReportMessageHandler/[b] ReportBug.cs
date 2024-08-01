using Server.Reawakened.Core.Services;
using Server.Reawakened.Network.Protocols;
using System.Text;

namespace Protocols.External._o__ReportMessageHandler;
public class ReportBug : ExternalProtocol
{
    public override string ProtocolName => "ob";

    public DiscordHandler DiscordHandler { get; set; }

    public override void Run(string[] message)
    {
        var detail = Encoding.UTF8.GetString(Convert.FromBase64String(message[5].Replace("detail$", "")));
        var summary = Encoding.UTF8.GetString(Convert.FromBase64String(message[6].Replace("summary$", "")));
        // Commented out due to a protocol error
        //var systeminfo = Encoding.UTF8.GetString(Convert.FromBase64String(message[7].Replace("systemInfo$", "")));

        var reportId = Guid.NewGuid();

        DiscordHandler.SendBugReport(reportId.ToString(), detail, summary, "N/A");

        SendXt("ob", "1");
    }
}
