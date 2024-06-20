using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Misc;
public class SetEwSubscribed : SlashCommand
{
    public override string CommandName => "/SetEwSubscribed";

    public override string CommandDescription => "Change membership status.";

    public override List<ParameterModel> Parameters =>
    [
        new ParameterModel()
        {
            Name = "status",
            Description = "A number 0/1 for membership status.",
            Optional = false
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Moderator;

    public override void Execute(Player player, string[] args)
    {
        if (args.Length != 2 || !int.TryParse(args[1], out var status))
        {
            Log("Invalid number provided.", player);
            return;
        }

        player.UserInfo.Write.Member = status == 1;

        player.SendXt("cb", status);

        Log("Set your membership status to " + (status == 1 ? "true" : "false"), player);
    }
}
