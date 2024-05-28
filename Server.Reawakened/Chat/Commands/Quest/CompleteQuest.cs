using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Quest;
public class CompleteQuest : SlashCommand
{
    public override string CommandName => "/CompleteQuest";
    public override string CommandDescription => "This marks the provided quest as completed.";
    public override List<ParameterModel> Parameters => [
        new ParameterModel() {
            Name = "questname",
            Description = "The quest name to be marked as completed.",
            Optional = false
        }
    ];
    public override AccessLevel AccessLevel => AccessLevel.Moderator;

    public QuestCatalog QuestCatalog { get; set; }

    public override void Execute(Player player, string[] args)
    {
        if (args.Length != 2)
            return;

        var questData = GetQuest(player, args);

        if (questData == null)
            return;

        var questModel = player.Character.Data.QuestLog.FirstOrDefault(x => x.Id == questData.Id);

        if (questModel != null)
            player.Character.Data.QuestLog.Remove(questModel);

        player.Character.Data.CompletedQuests.Add(questData.Id);
        Log($"Added quest {questData.Name} with id {questData.Id} to completed quests.", player);
    }

    public QuestDescription GetQuest(Player player, string[] args)
    {
        if (args.Length == 1)
        {
            Log("Please provide a quest id.", player);
            return null;
        }

        if (args.Length != 2)
            return null;

        if (!int.TryParse(args[1], out var questId))
        {
            Log("Please provide a valid quest id.", player);
            return null;
        }

        var questData = QuestCatalog.GetQuestData(questId);

        if (questData == null)
        {
            Log("Please provide a valid quest id.", player);
            return null;
        }

        if (player.Character.Data.CompletedQuests.Contains(questData.Id))
        {
            Log($"Quest {questData.Name} with id {questData.Id} has been completed already.", player);
            return null;
        }

        return questData;
    }
}
