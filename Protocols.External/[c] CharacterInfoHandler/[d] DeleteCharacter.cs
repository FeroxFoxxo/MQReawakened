using Server.Reawakened.Database.Characters;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._c__CharacterInfoHandler;

public class DeleteCharacter : ExternalProtocol
{
    public override string ProtocolName => "cd";

    public CharacterHandler CharacterHandler { get; set; }

    public override void Run(string[] message)
    {
        var character = CharacterHandler.GetCharacterFromName(message[5]);

        if (character != null)
            if (character.UserUuid == Player.UserId)
            {
                CharacterHandler.DeleteCharacter(character.Id, Player.UserInfo);
                SendXt("cd", 0);
                return;
            }

        SendXt("cd", 1);
    }
}
