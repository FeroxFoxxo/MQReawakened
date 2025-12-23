using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Moderation;
public class Disable : SlashCommand
{
    public override string CommandName => "/disable";

    public override string CommandDescription => "Disables a specified feature for the game.";

    public override List<ParameterModel> Parameters => 
    [
        new ParameterModel() 
        {
            Name = "feature",
            Description = "The feature to be disabled.",
            Optional = false,
            Options = 
            [
                new OptionModel()
                {
                    Name = "all",
                    Description = "Disables all features, such as Chat, Trading, Vendors and Gifting"
                },
                new OptionModel()
                {
                    Name = "chat",
                    Description = "Disables chat"
                },
                new OptionModel()
                {
                    Name = "vendor",
                    Description = "Disables vendors"
                },
                new OptionModel()
                {
                    Name = "trading",
                    Description = "Disables trading"
                },
                new OptionModel()
                {
                    Name = "gifting",
                    Description = "Disables gifting"
                }
            ]
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Moderator;

    public ServerRConfig ServerRConfig { get; set; }

    public override void Execute(Player player, string[] args)
    {
        if (args.Length == 0)
        {
            Log("You must provide a feature to disable.", player);
            return;
        }

        var feature = args[1];

        switch (feature)
        {
            case "chat":
                ServerRConfig.Chat = false;
                break;
            case "vendor":
                ServerRConfig.Vendor = false;
                break;
            case "trading":
                ServerRConfig.Trading = false;
                break;
            case "gifting":
                ServerRConfig.Gifting = false;
                break;
            default:
                ServerRConfig.Chat = false;
                ServerRConfig.Vendor = false;
                ServerRConfig.Trading = false;
                ServerRConfig.Gifting = false;
                break;
        }

        Log($"Disabled {feature} feature(s).", player);
    }
}
