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
public class UnMute : SlashCommand
{
    public override string CommandName => "/unmute";

    public override string CommandDescription => "Unmute a player";

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
            online.Account.SetMuted(false);

            var type = new JsonData()
            {
                ["type"] = "UNSILENCE"
            };

            online.SendXt("yM", type.ToJson());

            Log($"Unmuted player {online.Account.Username}.", player);
        }
        else
        {
            var target = AccountHandler.GetAccountFromId(id);

            if (target != null)
            {
                target.SetMuted(false);

                AccountHandler.Update(target.Write);

                Log($"Unmuted player {target.Username}.", player);
            }
        }
    }
}
