using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Models.Groups;

namespace Protocols.External._p__GroupHandler;

public class InvitePlayer : ExternalProtocol
{
    public override string ProtocolName => "pi";

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();

        player.Group ??= new GroupModel(NetState);

        var characterName = message[5];

        var invitedCharacter = player.Room.Clients.Values
            .First(c => c.Get<Player>().Character.Data.CharacterName == characterName);

        invitedCharacter.SendXt("pi", player.Group.LeaderCharacterName);
    }
}
