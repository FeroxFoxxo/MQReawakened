using Server.Base.Core.Models;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Models.Levels;

namespace Protocols.External._l__ExtLevelEditor;

public class RoomInstance : ExternalProtocol
{
    public override string ProtocolName => "li";

    public InternalRwConfig Config { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();

        var roomInstance = new RoomInstanceInfoModel(player, Config);

        SendXt("li", roomInstance);
    }
}
