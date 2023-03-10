using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;

namespace Protocols.External._f__FriendsHandler;

public class FriendInvite : ExternalProtocol
{
    public override string ProtocolName => "fv";

    public PlayerHandler PlayerHandler { get; set; }

    public override void Run(string[] message)
    {
        var characterName = message[5];

        var invitedCharacter = PlayerHandler.PlayerList
            .FirstOrDefault(p => p.Character.Data.CharacterName == characterName);

        invitedCharacter?.SendXt("fv",
            Player.Character.Data.CharacterName,
            Player.Room.LevelInfo.InGameName
        );
    }
}
