using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._c__CharacterInfoHandler;

public class DeleteCharacter : ExternalProtocol
{
    public override string ProtocolName => "cd";

    public override void Run(string[] message)
    {
        var character = Player.GetCharacterFromName(message[5]);
        var characterExists = character != null;

        if (characterExists)
            Player.DeleteCharacter(character.Data.CharacterId);

        SendXt("cd", characterExists ? 0 : 1);
    }
}
