using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.BundlesInternal;
using Server.Reawakened.XMLs.Enums;

namespace Protocols.External._f__FriendsHandler;

public class InviteResponse : ExternalProtocol
{
    public override string ProtocolName => "fr";

    public InternalAchievement InternalAchievement { get; set; }
    public ILogger<InviteResponse> Logger { get; set; }

    public override void Run(string[] message)
    {
        var accepted = message[6] == "1";
        var status = int.Parse(message[7]);

        var frienderName = message[5];
        var friender = Player.PlayerContainer.GetPlayerByName(frienderName);

        if (friender == null)
            return;

        if (accepted)
        {
            friender.CheckAchievement(AchConditionType.AddFriend, string.Empty, InternalAchievement, Logger);
            Player.CheckAchievement(AchConditionType.AddFriend, string.Empty, InternalAchievement, Logger);

            friender.Character.Data.Friends.Add(Player.CharacterId);
            Player.Character.Data.Friends.Add(friender.CharacterId);

            var playerData = friender.Character.Data.GetFriends().PlayerList.First(x => x.CharacterId == Player.CharacterId);

            friender.SendXt("fr", friender.CharacterName, Player.CharacterName, playerData);

            const bool isSuccess = true;

            var friendData = Player.Character.Data.GetFriends().PlayerList.First(x => x.CharacterId == friender.CharacterId);

            Player.SendXt("fa", friendData, isSuccess ? "1" : "0");
        }
        else
        {
            friender.SendXt("fc", Player.CharacterName, status);
        }
    }
}
