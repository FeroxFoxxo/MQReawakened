using Server.Base.Core.Configs;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Models.Levels;

namespace Protocols.External._l__ExtLevelEditor;

public class RoomInstance : ExternalProtocol
{
    public override string ProtocolName => "li";

    public InternalRwConfig Config { get; set; }

    public override void Run(string[] message) =>
        SendXt("li", new RoomInstanceInfoModel(Player, Config));
}
