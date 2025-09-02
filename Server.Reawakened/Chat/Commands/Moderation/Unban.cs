using Server.Base.Accounts.Enums;
using Server.Base.Accounts.Extensions;
using Server.Base.Database.Accounts;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Data.Commands;
using System.Security.Principal;
using UnityEngine;

namespace Server.Reawakened.Chat.Commands.Moderation;
public class UnBan : SlashCommand
{
    public override string CommandName => "/unban";

    public override string CommandDescription => "Unban a player";

    public override List<ParameterModel> Parameters =>
    [
        new ParameterModel()
        {
            Name = "accountId",
            Description = "The player account id",
            Optional = false
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

        if (online != null)
        {
            online.Account.SetBanned(false);

            Log($"Unbanned player {online.Account.Username}.", player);
        }
        else
        {
            var target = AccountHandler.GetAccountFromId(id);

            if (target != null)
            {
                target.SetBanned(false);

                AccountHandler.Update(target.Write);

                Log($"Unbanned player {target.Username}.", player);
            }
        }
    }
}
