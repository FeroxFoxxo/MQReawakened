using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;

namespace Protocols.External._p__GroupHandler;

public class JoinGroup : ExternalProtocol
{
    public override string ProtocolName => "pj";

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();
        var joinerName = player.Character.Data.CharacterName;

        var leaderName = message[5];
        var accepted = message[6] == "1";
        var status = int.Parse(message[7]);

        var leaderCharacter = player.Room.Clients.Values
            .First(c => c.Get<Player>().Character.Data.CharacterName == leaderName);

        if (accepted)
        {
            var leaderPlayer = leaderCharacter.Get<Player>();

            leaderPlayer.Group.GroupMembers.Add(NetState);
            player.Group = leaderPlayer.Group;

            foreach (var member in player.Group.GroupMembers)
                member.SendXt("pj", player.Group, joinerName);
        }
        else
        {
            leaderCharacter.SendXt("px", joinerName, status);
        }
    }
}
