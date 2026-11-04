using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Misc;

public class GiveCash : SlashCommand
{
    public override string CommandName => "/givecash";

    public override string CommandDescription => "Adds Monkeycash to a user (Default self, 1000 MC).";

    public override List<ParameterModel> Parameters =>
    [
        new ParameterModel()
        {
            Name = "amount",
            Description = "The amount of Monkeycash to receive.",
            Optional = true
        },

        new ParameterModel()
        {
            Name = "character",
            Description = "Player to give Monkeycash to.",
            Optional = true
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Owner;

    public InternalAchievement InternalAchievement { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public ILogger<GiveBananas> Logger { get; set; }
    public ServerRwConfig ServerRwConfig { get; set; }
    public PlayerContainer PlayerContainer { get; set; }
    public CharacterHandler CharacterHandler { get; set; }

    public override void Execute(Player player, string[] args)
    {
        var targetName = player.CharacterName;
        Player target;

        if (args.Length < 2)
        {
            player.AddNCash(1000);
            return;
        }

        if (!int.TryParse(args[1], out var amount))
        {
            Log("Invalid amount provided, defaulting to 1000 MC", player);
            amount = 1000;
        }

        CharacterDbEntry targetImage = null;

        if (args.Length >= 3)
        {
            targetName = string.Join(" ", args.Skip(2));
            target = PlayerContainer.GetPlayerByName(targetName);
            if (target is null)
            {
                var targetEntity = CharacterHandler.GetCharacterFromName(targetName);

                if (targetEntity is null)
                {
                    Log("Invalid player name.", player);
                    return;
                }

                targetImage = CharacterHandler.Get(targetEntity.Id);

                if (targetImage is null)
                {
                    Log("Invalid player name.", player);
                    return;
                }
            }
        }
        else
            target = player;

        if (target != null)
            target.AddNCash(amount);
        else
        {
            targetImage.NCash += amount;
            CharacterHandler.Update(targetImage);
        }

        Log("Successfully gave " + amount + " MC to " + targetName, player);
    }
}
