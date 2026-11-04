using LitJson;
using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Core.Services;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Moderation;
public class Kick : SlashCommand
{
    public override string CommandName => "/kick";

    public override string CommandDescription => "Kick a player for bad behavior.";

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

    public PlayerContainer PlayerContainer { get; set; }
    public DiscordHandler DiscordHandler { get; set; }

    public override void Execute(Player player, string[] args)
    {
        if (!int.TryParse(args[1], out var id))
        {
            Log("Invalid player account id provided.", player);
            return;
        }

        var online = PlayerContainer.GetPlayerByAccountId(id);

        if (online == null)
        {
            Log("The provided player account is null.", player);
            return;
        }

        var reason = string.Empty;
        
        if (args.Length >= 3)
            reason = string.Join(" ", args.Skip(2));
        
        var type = new JsonData()
        {
            ["type"] = "KICK"
        };

        online.SendXt("yM", type.ToJson());

        Log($"Kicked player {online.Account.Username}.", player);
        
        DiscordHandler.SendPunishmentLog("Kicked", player.Account.Username, online.Account.Username, reason, string.Empty);
    }
}
