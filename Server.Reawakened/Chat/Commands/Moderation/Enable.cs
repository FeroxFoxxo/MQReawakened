using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Moderation;
public class Enable : SlashCommand
{
    public override string CommandName => "/enable";

    public override string CommandDescription => "Enables a specified feature for the game.";

    public override List<ParameterModel> Parameters =>
    [
        new ParameterModel()
        {
            Name = "feature",
            Description = "The feature to be enabled.",
            Optional = false,
            Options =
            [
                new OptionModel()
                {
                    Name = "all",
                    Description = "Enables all features such as Chat, Trading, Vendors and Gifting"
                },
                new OptionModel()
                {
                    Name = "chat",
                    Description = "Enables chat"
                },
                new OptionModel()
                {
                    Name = "vendor",
                    Description = "Enables vendors"
                },
                new OptionModel()
                {
                    Name = "trading",
                    Description = "Enables trading"
                },
                new OptionModel()
                {
                    Name = "gifting",
                    Description = "Enables gifting"
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
            Log("You must provide a feature to enable.", player);
            return;
        }

        var feature = args[1];

        switch (feature)
        {
            case "chat":
                ServerRConfig.Chat = true;
                break;
            case "vendor":
                ServerRConfig.Vendor = true;
                break;
            case "trading":
                ServerRConfig.Trading = true;
                break;
            case "gifting":
                ServerRConfig.Gifting = true;
                break;
            default:
                ServerRConfig.Chat = true;
                ServerRConfig.Vendor = true;
                ServerRConfig.Trading = true;
                ServerRConfig.Gifting = true;
                break;
        }

        Log($"Enabled {feature} feature(s).", player);
    }
}
