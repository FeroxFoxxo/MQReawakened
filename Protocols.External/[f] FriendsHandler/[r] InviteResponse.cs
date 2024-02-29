using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;

namespace Protocols.External._f__FriendsHandler;

public class InviteResponse : ExternalProtocol
{
    public override string ProtocolName => "fr";

    public PlayerContainer PlayerContainer { get; set; }

    public override void Run(string[] message)
    {
        var accepted = message[6] == "1";
        var status = int.Parse(message[7]);

        var frienderName = message[5];
        var friender = PlayerContainer.GetPlayerByName(frienderName);

        if (friender == null)
            return;

        if (accepted)
        {
            friender.Character.Data.Friends.Add(Player.Account.Id);
            Player.Character.Data.Friends.Add(Player.Account.Id);

            friender.SendXt("fr",
                friender.CharacterName,
                Player.CharacterName,
                friender.Character.Data.GetFriends()
            );

            const bool isSuccess = true;

            Player.SendXt("fa",
                Player.Character.Data.GetFriends(),
                isSuccess ? "1" : "0"
            );
        }
        else
        {
            friender.SendXt("fc", Player.CharacterName, status);
        }
    }
}
