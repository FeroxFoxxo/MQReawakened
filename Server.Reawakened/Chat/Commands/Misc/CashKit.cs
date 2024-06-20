using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Misc;
public class CashKit : SlashCommand
{
    public override string CommandName => "/CashKit";

    public override string CommandDescription => "This will give 100k of bananas and monkey cash.";

    public override List<ParameterModel> Parameters => [];

    public override AccessLevel AccessLevel => AccessLevel.Player;

    public ServerRConfig ServerRConfig { get; set; }
    public InternalAchievement InternalAchievement { get; set; }
    public ILogger<CashKit> Logger { get; set; }

    public override void Execute(Player player, string[] args)
    {
        player.AddBananas(ServerRConfig.CashKitAmount, InternalAchievement, Logger);
        player.AddNCash(ServerRConfig.CashKitAmount);

        Log($"{player.Character.CharacterName} received {ServerRConfig.CashKitAmount} " +
            $"banana{(ServerRConfig.CashKitAmount > 1 ? "s" : string.Empty)} & monkey cash!", player);
    }
}
