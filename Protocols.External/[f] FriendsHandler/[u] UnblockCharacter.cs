using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Services;

namespace Protocols.External._f__FriendsHandler;
public class UnblockCharacter : ExternalProtocol
{
    public override string ProtocolName => "fu";

    public CharacterHandler CharacterHandler { get; set; }

    public override void Run(string[] message)
    {
        var characterName = message[5];
        var blocked = CharacterHandler.GetCharacterFromName(characterName);

        Player.Character.Data.Blocked.Remove(blocked.Id);

        Player.SendXt("fu", characterName);
    }
}
