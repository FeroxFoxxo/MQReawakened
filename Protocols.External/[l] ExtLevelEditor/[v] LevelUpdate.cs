using Microsoft.Extensions.Logging;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._l__ExtLevelEditor;

public class LevelUpdate : ExternalProtocol
{
    public override string ProtocolName => "lv";
    
    public ILogger<LevelUpdate> Logger { get; set; }
    public LevelHandler LevelHandler { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();

        player.GetCurrentLevel(LevelHandler).SendCharacterInfo(player, NetState);
        SendXt("lv", 0, string.Empty);
    }
}
