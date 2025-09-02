using Server.Base.Accounts.Enums;
using Server.Base.Accounts.Extensions;
using Server.Base.Database.Accounts;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Moderation;
public class Mute : SlashCommand
{
    public override string CommandName => "/mute";

    public override string CommandDescription => "Mute a player for bad behavior.";

    public override List<ParameterModel> Parameters =>
    [
        new ParameterModel()
        {
            Name = "accountId",
            Description = "The player account id",
            Optional = false
        },
        new ParameterModel()
        {
            Name = "duration",
            Description = "The mute duration in a format similar to 1d1h1m1s etc.",
            Optional = true
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Moderator;

    public AccountHandler AccountHandler { get; set; }
    public PlayerContainer PlayerContainer { get; set; }

    public override void Execute(Player player, string[] args)
    {
        if (!int.TryParse(args[1], out var id))
        {
            Log("Invalid player id provided.", player);
            return;
        }

        var online = PlayerContainer.GetPlayerByAccountId(id);
        TimeSpan time;

        if (online != null)
        {
            time = args.Length < 3 ? TimeSpan.MaxValue : online.Account.ParseTime(args[2]);

            online.Account.SetMuted(true);
            online.Account.SetMuteTags(null, DateTime.Now, time);

            Log($"Muted {online.Account.Username}'s account{online.Account.FormatMuteTime()}.", player);

            online.SendWarningMessage("mute");

            Log($"You have been muted{online.Account.FormatMuteTime()}.", online);
            return;
        }
        else
        {
            var target = AccountHandler.GetAccountFromId(id);

            if (target != null)
            {
                time = args.Length < 3 ? TimeSpan.MaxValue : target.ParseTime(args[2]);

                target.SetMuted(true);
                target.SetMuteTags(null, DateTime.Now, time);

                AccountHandler.Update(target.Write);

                Log($"Muted {target.Username}'s account{target.FormatMuteTime()}.", player);
            }
        }
    }
}
