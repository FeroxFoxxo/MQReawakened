using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;

namespace Protocols.External._p__GroupHandler;

public class JoinGroup : ExternalProtocol
{
    public override string ProtocolName => "pj";

    public PlayerHandler PlayerHandler { get; set; }

    public override void Run(string[] message)
    {
        var joinerName = Player.Character.Data.CharacterName;

        var leaderName = message[5];
        var accepted = message[6] == "1";
        var status = int.Parse(message[7]);

        var leaderPlayer = PlayerHandler.PlayerList
            .FirstOrDefault(p => p.Character.Data.CharacterName == leaderName);

        if (leaderPlayer == null)
            return;

        if (accepted)
        {
            leaderPlayer.Group.GroupMembers.Add(Player);
            Player.Group = leaderPlayer.Group;

            foreach (var member in Player.Group.GroupMembers)
                member.SendXt("pj", Player.Group, joinerName);
        }
        else
        {
            leaderPlayer.SendXt("px", joinerName, status);
        }
    }
}
