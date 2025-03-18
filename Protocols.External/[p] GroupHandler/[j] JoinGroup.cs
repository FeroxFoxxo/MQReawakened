using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Achievements;

namespace Protocols.External._p__GroupHandler;

public class JoinGroup : ExternalProtocol
{
    public override string ProtocolName => "pj";

    public PlayerContainer PlayerContainer { get; set; }
    public InternalAchievement InternalAchievement { get; set; }
    public ILogger<JoinGroup> Logger { get; set; }

    public override void Run(string[] message)
    {
        var joinerName = Player.CharacterName;

        var leaderName = message[5];
        var leaderPlayer = PlayerContainer.GetPlayerByName(leaderName);

        var accepted = message[6] == "1";
        var status = int.Parse(message[7]);

        if (leaderPlayer == null)
            return;

        if (accepted)
        {
            leaderPlayer.TempData.Group.AddPlayer(Player);
            Player.TempData.Group = leaderPlayer.TempData.Group;

            foreach (var member in Player.TempData.Group.GetMembers())
                member.SendXt("pj", Player.TempData.Group, joinerName);

            leaderPlayer.CheckAchievement(AchConditionType.JoinGroup, [], InternalAchievement, Logger);
            Player.CheckAchievement(AchConditionType.JoinGroup, [], InternalAchievement, Logger);
        }
        else
        {
            leaderPlayer.SendXt("px", joinerName, status);
        }
    }
}
