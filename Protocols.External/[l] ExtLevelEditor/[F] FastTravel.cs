using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Enums;

namespace Protocols.External._l__ExtLevelEditor;

public class FastTravel : ExternalProtocol
{
    public override string ProtocolName => "lF";

    public ILogger<FastTravel> Logger { get; set; }
    public WorldGraph WorldGraph { get; set; }

    public override void Run(string[] message)
    {
        var character = Player.Character;

        var levelId = int.Parse(message[5]);
        var goId = int.Parse(message[6]);
        var newLevelId = WorldGraph.GetLevelFromPortal(levelId, goId);

        character.SetLevel(newLevelId, Logger);

        Player.CheckAchievement(AchConditionType.ExploreTrail, string.Empty, Logger);
        Player.CheckAchievement(AchConditionType.ExploreTrail, Player.Room.LevelInfo.Name, Logger);

        Player.SendLevelChange();
    }
}
