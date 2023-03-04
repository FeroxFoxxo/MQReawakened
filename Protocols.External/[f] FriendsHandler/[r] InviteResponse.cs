using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;

namespace Protocols.External._f__FriendsHandler;

public class InviteResponse : ExternalProtocol
{
    public override string ProtocolName => "fr";

    public PlayerHandler PlayerHandler { get; set; }

    public override void Run(string[] message)
    {
        var frienderName = message[5];
        var accepted = message[6] == "1";
        var status = int.Parse(message[7]);

        var friender = PlayerHandler.PlayerList
            .First(p => p.Character.Data.CharacterName == frienderName);

        if (accepted)
        {
            friender.Character.Data.Friends.Add(Player.UserId);
            Player.Character.Data.Friends.Add(friender.UserId);

            friender.SendXt("fr",
                friender.Character.Data.CharacterName,
                Player.Character.Data.CharacterName,
                friender.Character.Data.GetFriends()
            );

            Player.SendXt("fr",
                friender.Character.Data.CharacterName,
                Player.Character.Data.CharacterName,
                Player.Character.Data.GetFriends()
            );
        }
        else
        {
            friender.SendXt("fc", Player.Character.Data.CharacterName, status);
        }
    }
}
