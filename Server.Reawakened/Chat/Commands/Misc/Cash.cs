using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Misc;
public class Cash : SlashCommand
{
    public override string CommandName => "/cash";

    public override string CommandDescription => "Adds cash or default to cash kit amount.";

    public override List<ParameterModel> Parameters =>
    [
        new ParameterModel()
        {
            Name = "amount",
            Description = "The amount of cash to receive.",
            Optional = true
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Player;

    public ServerRConfig ServerRConfig { get; set; }

    public override void Execute(Player player, string[] args)
    {
        if (args.Length != 2 || !int.TryParse(args[1], out var amount))
        {
            Log("Invalid amount provided, defaulting to cash kit...", player);
            amount = ServerRConfig.CashKitAmount;
        }

        player.AddNCash(amount);
    }
}
