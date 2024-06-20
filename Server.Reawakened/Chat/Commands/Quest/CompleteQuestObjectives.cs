using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Quest;
public class CompleteQuestObjectives : SlashCommand
{
    public override string CommandName => "/CompleteQuestObjectives";

    public override string CommandDescription => "Completes the provided quest's objectives.";

    public override List<ParameterModel> Parameters =>
    [
        new ParameterModel()
        {
            Name = "name",
            Description = "The quest to have it's objectives marked as completed. (i.e. OOTU_0_07)",
            Optional = false
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Player;

    public QuestCatalog QuestCatalog { get; set; }

    public override void Execute(Player player, string[] args)
    {
        if (args.Length != 2)
            return;

        var questId = QuestCatalog.GetQuestIdFromName(args[1]);

        if (!QuestCatalog.QuestCatalogs.TryGetValue(questId, out var quest))
        {
            Log("Invalid quest provided.", player);
            return;
        }

        var questModel = player.Character.QuestLog.FirstOrDefault(x => x.Id == quest.Id);

        foreach (var objective in questModel.Objectives)
        {
            objective.Value.CountLeft = 0;
            objective.Value.Completed = true;
        }

        player.UpdateNpcsInLevel(quest);

        Log($"The quest {quest.Name} with id {quest.Id} is now ready to turn in.", player);
    }
}
