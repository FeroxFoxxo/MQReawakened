using Server.Reawakened.Network.Protocols;

namespace Protocols.External._f__FriendsHandler;
public class DeleteResponse : ExternalProtocol
{
    public override string ProtocolName => "fd";

    public override void Run(string[] message)
    {
        var characterName = message[5];
        var friend = Player.PlayerContainer.GetPlayerByName(characterName);

        if (friend != null)
        {
            Player.Character.Data.Friends.Remove(friend.CharacterId);
            friend.Character.Data.Friends.Remove(Player.CharacterId);

            SendXt("fd", characterName, "1");
        }
        else
            SendXt("fd", characterName, "0");
    }
}
