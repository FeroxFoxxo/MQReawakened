using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Quest;
public class FindQuest : SlashCommand
{
    public override string CommandName => "/FindQuest";

    public override string CommandDescription => "Allows you to find a quest name.";

    public override List<ParameterModel> Parameters =>
    [
        new ParameterModel()
        {
            Name = "questName",
            Description = "The quest to find.",
            Optional = false
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Player;

    public QuestCatalog QuestCatalog { get; set; }

    public override void Execute(Player player, string[] args)
    {
        var name = string.Join(" ", args.Skip(1)).ToLower();

        var closestQuest = QuestCatalog.QuestCatalogs.FirstOrDefault(q => q.Value.Title.Equals(name, StringComparison.OrdinalIgnoreCase)).Value;

        if (closestQuest == null)
        {
            Log($"Could not find quest with name '{name}'.", player);
            return;
        }

        Log($"Found quest: '{closestQuest.Name}'.", player);
    }
}
