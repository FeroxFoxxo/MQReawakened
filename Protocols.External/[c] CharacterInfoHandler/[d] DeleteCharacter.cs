using Server.Reawakened.Core.Network.Protocols;
using Server.Reawakened.Players;

namespace Protocols.External._c__CharacterInfoHandler;

public class DeleteCharacter : ExternalProtocol
{
    public override string ProtocolName => "cd";

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();

        var characterName = message[5];

        var character = player.UserInfo.Characters
            .FirstOrDefault(c => c.Value.CharacterName == characterName);

        var characterExists = character.Value != null;

        if (characterExists)
            player.UserInfo.Characters.Remove(character.Key);
         
        SendXt("cd", characterExists ? 0 : 1);
    }
}
