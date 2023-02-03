using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._c__CharacterInfoHandler;

public class DeleteCharacter : ExternalProtocol
{
    public override string ProtocolName => "cd";

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();

        var character = player.GetCharacterFromName(message[5]);
        var characterExists = character != null;

        if (characterExists)
            player.DeleteCharacter(character.Data.CharacterId);

        SendXt("cd", characterExists ? 0 : 1);
    }
}
