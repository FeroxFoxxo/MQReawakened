using LitJson;
using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Moderation;
public class UnMute : SlashCommand
{
    public override string CommandName => "/unmute";

    public override string CommandDescription => "Unmute a player";

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

        var type = new JsonData()
        {
            ["type"] = "UNSILENCE"
        };

        target.SendXt("yM", type.ToJson());

        Log($"Unmuted player {target.Account.Username}.", player);
    }
}
