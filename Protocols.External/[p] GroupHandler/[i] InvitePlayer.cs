using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Models.Groups;

namespace Protocols.External._p__GroupHandler;

public class InvitePlayer : ExternalProtocol
{
    public override string ProtocolName => "pi";

    public PlayerHandler PlayerHandler { get; set; }

    public override void Run(string[] message)
    {
        Player.TempData.Group ??= new GroupModel(Player);

        var characterName = message[5];
        var invitedCharacter = PlayerHandler.GetPlayerByName(characterName);

        invitedCharacter?.SendXt("pi", Player.TempData.Group.GetLeaderName());
    }
}
