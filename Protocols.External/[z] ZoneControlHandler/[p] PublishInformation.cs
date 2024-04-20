using Server.Reawakened.BundleHost.Configs;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;

namespace Protocols.External._z__ZoneControlHandler;

public class PublishInformation : ExternalProtocol
{
    public override string ProtocolName => "zp";

    public AssetBundleRConfig Config { get; set; }

    public override void Run(string[] message) =>
        SendXt("zp", GetPublishConfigs(Config.PublishConfigs));

    private static string GetPublishConfigs(Dictionary<string, string> configs)
    {
        var sb = new SeparatedStringBuilder(',');

        foreach (var config in configs)
        {
            var sb2 = new SeparatedStringBuilder('=');

            sb2.Append(config.Key);
            sb2.Append(config.Value);

            sb.Append(sb2.ToString());
        }

        return sb.ToString();
    }
}
