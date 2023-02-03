using Server.Reawakened.Network.Protocols;

namespace Protocols.External._l__ExtLevelEditor;

public class LevelUpdate : ExternalProtocol
{
    public override string ProtocolName => "lv";

    public override void Run(string[] message)
    {
        SendXt("lv", 0, string.Empty);
    }
}
