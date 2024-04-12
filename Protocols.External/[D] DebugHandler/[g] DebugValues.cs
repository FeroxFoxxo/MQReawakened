using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;

namespace Protocols.External._D__DebugHandler;

public class DebugValues : ExternalProtocol
{
    public override string ProtocolName => "Dg";

    public ServerRConfig Config { get; set; }

    public override void Run(string[] message) =>
        SendXt("Dg", GetDebugValues());

    public string GetDebugValues()
    {
        var sb = new SeparatedStringBuilder('|');

        foreach (var debug in Config.DefaultDebugVariables)
        {
            sb.Append((int)debug.Key);
            sb.Append(debug.Value ? "On" : "Off");
        }

        return sb.ToString();
    }
}
