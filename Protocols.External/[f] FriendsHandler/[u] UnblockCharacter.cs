using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;

namespace Protocols.External._f__FriendsHandler;
public class UnblockCharacter : ExternalProtocol
{
    public override string ProtocolName => "fu";

    public override void Run(string[] message)
    {
        var characterName = message[5];
        var blocked = Player.CharacterHandler.GetCharacterFromName(characterName);

        Player.Character.Data.Blocked.Remove(blocked.Id);

        Player.SendXt("fu", characterName);
    }
}
