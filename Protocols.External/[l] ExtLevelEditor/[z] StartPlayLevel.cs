using Server.Reawakened.Levels.Extensions;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models.Character;
using WorldGraphDefines;

namespace Protocols.External._l__ExtLevelEditor;

public class StartPlayLevel : ExternalProtocol
{
    public LevelHandler LevelHandler { get; set; }

    public override string ProtocolName => "lz";

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();

        var level = player.GetCurrentLevel(LevelHandler);
        player.JoinLevel(NetState, level, out var reason);

        SendXt("lz", reason.GetJoinReasonError(), level.LevelInfo.LevelId, level.LevelInfo.Name);

        if (player.GetCurrentCharacter().DiscoverTribe(level.LevelInfo))
            SendXt("cB", (int)level.LevelInfo.Tribe);
    }
}
