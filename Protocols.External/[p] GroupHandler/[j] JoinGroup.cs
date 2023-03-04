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
            var group = leaderCharacter.Get<Player>().Group;
            group.GroupMembers.Add(NetState);
            foreach (var member in group.GroupMembers)
                member.SendXt("pj", group, joinerName);
        }
        else
        {
            leaderCharacter.SendXt("px", joinerName, status);
        }
    }
}
