using LitJson;
using Server.Base.Accounts.Enums;
using Server.Base.Accounts.Extensions;
using Server.Base.Database.Accounts;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Moderation;
public class Ban : SlashCommand
{
    public override string CommandName => "/ban";

    public override string CommandDescription => "Ban a player for bad behavior.";

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
            Description = "The ban duration in a format similar to 1d1h1m1s etc.",
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
            Log("Invalid player account id provided.", player);
            return;
        }

        var online = PlayerContainer.GetPlayerByAccountId(id);
        TimeSpan time;

        if (online != null)
        {
            time = args.Length < 3 ? TimeSpan.MaxValue : online.Account.ParseTime(args[2]);

            var type = new JsonData()
            {
                ["type"] = "BAN"
            };

            online.SendXt("yM", type.ToJson());

            online.Account.SetBanned(true);
            online.Account.SetBanTags(null, DateTime.Now, time);

            Log($"Banned {online.Account.Username}'s account{online.Account.FormatBanTime()}.", player);
            return;
        }
        else
        {
            var target = AccountHandler.GetAccountFromId(id);

            if (target != null)
            {
                time = args.Length < 3 ? TimeSpan.MaxValue : target.ParseTime(args[2]);

                target.SetBanned(true);
                target.SetBanTags(null, DateTime.Now, time);

                AccountHandler.Update(target.Write);

                Log($"Banned {target.Username}'s account{target.FormatBanTime()}.", player);
                return;
            }
        }
    }
}
