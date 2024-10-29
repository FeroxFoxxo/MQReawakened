using LitJson;
using Server.Base.Accounts.Enums;
using Server.Base.Accounts.Extensions;
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
            Name = "playerId",
            Description = "The player character id",
            Optional = false
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Moderator;

    public PlayerContainer PlayerContainer { get; set; }

    public override void Execute(Player player, string[] args)
    {
        if (!int.TryParse(args[1], out var id))
        {
            Log("Invalid player id provided.", player);
            return;
        }

        var target = PlayerContainer.GetPlayersByCharacterId(id).FirstOrDefault();

        if (target == null)
        {
            Log("The provided player account is null.", player);
            return;
        }

        target.Account.SetBanned(true);

        var type = new JsonData()
        {
            ["type"] = "BAN"
        };

        target.SendXt("yM", type.ToJson());

        Log($"Banned player {target.Account.Username}.", player);
    }
}
