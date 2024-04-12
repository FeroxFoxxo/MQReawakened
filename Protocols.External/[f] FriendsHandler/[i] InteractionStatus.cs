using A2m.Server;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;

namespace Protocols.External._f__FriendsHandler;

public class InteractionStatus : ExternalProtocol
{
    public override string ProtocolName => "fi";

    public override void Run(string[] message)
    {
        var interactionStatus = int.Parse(message[5]);
        var broadcast = message[6] == "1";

        Player.Character.Data.InteractionStatus = (CharacterLightData.InteractionStatus)interactionStatus;

        if (broadcast)
            foreach (var roomPlayer in Player.Room.GetPlayers())
                roomPlayer.SendXt("fi", Player.CharacterName, interactionStatus);
        else
            SendXt("fi", Player.CharacterName, interactionStatus);
    }
}
