using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;

namespace Protocols.External._p__GroupHandler;

public class JoinGroup : ExternalProtocol
{
    public override string ProtocolName => "pj";

    public DatabaseContainer DatabaseContainer { get; set; }

    public override void Run(string[] message)
    {
        var joinerName = Player.CharacterName;

        var leaderName = message[5];
        var leaderPlayer = DatabaseContainer.GetPlayerByName(leaderName);

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
        }
        else
        {
            leaderPlayer.SendXt("px", joinerName, status);
        }
    }
}
