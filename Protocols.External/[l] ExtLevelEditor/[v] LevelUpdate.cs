using Server.Reawakened.Levels.Enums;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._l__ExtLevelEditor;

public class LevelUpdate : ExternalProtocol
{
    public override string ProtocolName => "lv";

    public LevelHandler LevelHandler { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();

        SendXt("lv", 0, string.Empty);

        var level = player.GetCurrentLevel(LevelHandler);

        player.SendPlayerData(CharacterInfoType.Portals, NetState, LevelHandler);
    }
}
