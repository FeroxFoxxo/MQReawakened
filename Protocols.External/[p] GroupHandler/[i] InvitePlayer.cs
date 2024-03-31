using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Groups;
using Server.Reawakened.XMLs.BundlesInternal;
using Server.Reawakened.XMLs.Enums;

namespace Protocols.External._p__GroupHandler;

public class InvitePlayer : ExternalProtocol
{
    public override string ProtocolName => "pi";

    public PlayerContainer PlayerContainer { get; set; }
    public InternalAchievement InternalAchievement { get; set; }
    public ILogger<InvitePlayer> Logger { get; set; }

    public override void Run(string[] message)
    {
        Player.TempData.Group ??= new GroupModel(Player);

        var characterName = message[5];
        var invitedCharacter = PlayerContainer.GetPlayerByName(characterName);

        if (!invitedCharacter.Character.Data.Blocked.Contains(Player.CharacterId))
        {
            invitedCharacter.CheckAchievement(AchConditionType.InviteGroup, string.Empty, InternalAchievement, Logger);
            Player.CheckAchievement(AchConditionType.InviteGroup, string.Empty, InternalAchievement, Logger);

            invitedCharacter?.SendXt("pi", Player.TempData.Group.GetLeaderName());
        }
    }
}
