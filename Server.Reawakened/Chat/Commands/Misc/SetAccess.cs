using Server.Base.Accounts.Enums;
using Server.Base.Database.Accounts;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Misc;
public class SetAccess : SlashCommand
{
    public override string CommandName => "/setaccess";

    public override string CommandDescription => "Allows you to set a player's access level. (Owner only)";

    public override List<ParameterModel> Parameters =>
    [
        new ParameterModel()
        {
            Name = "number",
            Description = "The access level.",
            Optional = true,
            Options = [
                new OptionModel()
                {
                    Name = "0",
                    Description = "The Player access level.",
                },
                new OptionModel()
                {
                    Name = "1",
                    Description = "The VIP access level."
                },
                new OptionModel()
                {
                    Name = "2",
                    Description = "The Moderator access level."
                },
                new OptionModel()
                {
                    Name = "3",
                    Description = "The Owner access level."
                }
            ]
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Owner;

    public AccountHandler AccountHandler { get; set; }

    public override void Execute(Player player, string[] args)
    {
        if (args.Length != 3)
            return;

        var target = AccountHandler.GetAccountFromId(int.Parse(args[1]));

        if (target == null)
        {
            Log("Please provide a valid character id.", player);
            return;
        }

        target.Write.AccessLevel = (AccessLevel)int.Parse(args[2]);

        Log($"Set {target.Username}'s access level to {target.AccessLevel}", player);
    }
}
