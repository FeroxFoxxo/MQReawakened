using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Quest;
public class AddAndCompleteQuest : SlashCommand
{
    public override string CommandName => "/addandcompletequest";

    public override string CommandDescription => "This marks the provided quest as completed.";

    public override List<ParameterModel> Parameters =>
    [
        new ParameterModel()
        {
            Name = "name",
            Description = "The quest name to be marked as completed. (i.e. OOTU_0_07)",
            Optional = false
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Player;

    public QuestCatalog QuestCatalog { get; set; }

    public override void Execute(Player player, string[] args)
    {
        if (args.Length != 2)
            return;

        var questData = GetQuest(player, args);

        if (questData == null)
            return;

        var questModel = player.Character.QuestLog.FirstOrDefault(x => x.Id == questData.Id);

        if (questModel != null)
            player.Character.QuestLog.Remove(questModel);

        player.Character.CompletedQuests.Add(questData.Id);
        Log($"Added quest {questData.Name} with id {questData.Id} to completed quests.", player);
    }

    public QuestDescription GetQuest(Player player, string[] args)
    {
        if (args.Length == 1)
        {
            Log("Please provide a quest name.", player);
            return null;
        }

        if (args.Length != 2)
            return null;

        var questId = QuestCatalog.GetQuestIdFromName(args[1]);

        var questData = QuestCatalog.GetQuestData(questId);

        if (questData == null)
        {
            Log("Please provide a valid quest name.", player);
            return null;
        }

        if (player.Character.CompletedQuests.Contains(questData.Id))
        {
            Log($"Quest {questData.Name} with id {questData.Id} has been completed already.", player);
            return null;
        }

        return questData;
    }
}
