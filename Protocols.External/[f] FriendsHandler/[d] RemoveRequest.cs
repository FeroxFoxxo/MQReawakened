using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;

namespace Protocols.External._f__FriendsHandler;
public class DeleteResponse : ExternalProtocol
{
    public override string ProtocolName => "fd";

    public override void Run(string[] message)
    {
        var characterName = message[5];
        var friend = Player.CharacterHandler.GetCharacterFromName(characterName);

        if (friend != null)
        {
            Player.Character.Data.Friends.Remove(friend.Id);
            friend.Data.Friends.Remove(Player.CharacterId);

            Player.SendXt("fd", characterName, "1");
        }
        else
            Player.SendXt("fd", characterName, "0");
    }
}
