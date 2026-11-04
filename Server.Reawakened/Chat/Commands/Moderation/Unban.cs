using Server.Base.Accounts.Enums;
using Server.Base.Accounts.Extensions;
using Server.Base.Database.Accounts;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Core.Services;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Data.Commands;

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
        },
        new ParameterModel()
        {
            Name = "reason",
            Description = "The reason",
            Optional = true
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Moderator;

    public AccountHandler AccountHandler { get; set; }
    public DiscordHandler DiscordHandler { get; set; }
    
    public override void Execute(Player player, string[] args)
    {
        if (!int.TryParse(args[1], out var id))
        {
            Log("Invalid player account id provided.", player);
            return;
        }

        var target = AccountHandler.GetAccountFromId(id);

        var reason = string.Empty;
        
        if (args.Length >= 3)
            reason = string.Join(" ", args.Skip(2));
        
        if (target != null)
        {
            target.SetBanned(false);

            AccountHandler.Update(target.Write);

            Log($"Unbanned player {target.Username}.", player);
            
            DiscordHandler.SendPunishmentLog("Unbanned", player.Account.Username, target.Username, reason, string.Empty);
        }
    }
}
