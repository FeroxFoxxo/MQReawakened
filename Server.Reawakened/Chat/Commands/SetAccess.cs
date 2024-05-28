using Server.Base.Accounts.Enums;
using Server.Base.Accounts.Services;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands;
public class SetAccess : SlashCommand
{
    public override string CommandName => "/SetAccess";

    public override string CommandDescription => "Allows you to set a player's access level. (Owner only)";

    public override List<ParameterModel> Parameters => [
        new ParameterModel() {
            Name = "0",
            Description = "The Player access level.",
            Optional = true
        },
        new ParameterModel() {
            Name = "1",
            Description = "The VIP access level.",
            Optional = true
        },
        new ParameterModel() {
            Name = "2",
            Description = "The Moderator access level.",
            Optional = true
        },
        new ParameterModel() {
            Name = "3",
            Description = "The Owner access level.",
            Optional = true
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Owner;

    public AccountHandler AccountHandler { get; set; }

    public override void Execute(Player player, string[] args)
    {
        if (args.Length != 3)
            return;

        var target = AccountHandler.Get(int.Parse(args[1]));

        if (target == null)
        {
            Log("Please provide a valid character id.", player);
            return;
        }

        target.AccessLevel = (AccessLevel)int.Parse(args[2]);

        Log($"Set {target.Username}'s access level to {target.AccessLevel}", player);
    }
}
