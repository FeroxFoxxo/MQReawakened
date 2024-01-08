using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Services;

namespace Protocols.External._c__CharacterInfoHandler;

public class DeleteCharacter : ExternalProtocol
{
    public override string ProtocolName => "cd";

    public CharacterHandler CharacterHandler { get; set; }

    public override void Run(string[] message)
    {
        var character = CharacterHandler.GetCharacterFromName(message[5]);
        var characterExists = character != null;

        if (characterExists)
            Player.DeleteCharacter(character.Id, CharacterHandler);

        SendXt("cd", characterExists ? 0 : 1);
    }
}
