using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;

namespace Protocols.External._f__FriendsHandler;

public class FriendInvite : ExternalProtocol
{
    public override string ProtocolName => "fv";

    public PlayerHandler PlayerHandler { get; set; }

    public override void Run(string[] message)
    {
        var characterName = message[5];
        var invitedCharacter = PlayerHandler.GetPlayerByName(characterName);

        invitedCharacter?.SendXt("fv",
            Player.CharacterName,
            Player.Room.LevelInfo.InGameName
        );
    }
}
