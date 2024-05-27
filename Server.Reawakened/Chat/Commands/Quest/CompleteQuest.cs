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
    public override List<ParameterModel> Arguments => [
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

        var questId = QuestCatalog.GetQuestIdFromName(args[1]);
        var quest = QuestCatalog.GetQuestData(questId);

        if (quest == null)
            return;

        var questModel = player.Character.Data.QuestLog.FirstOrDefault(q => q.Id == quest.Id);

        if (questModel == null || !player.Character.Data.QuestLog.Contains(questModel))
        {
            Log($"The {quest.Name} quest with id {quest.Id} is not in your Quest Log.", player);
            return;
        }

        if (player.Character.Data.CompletedQuests.Contains(quest.Id))
        {
            Log($"The {quest.Name} quest with id {quest.Id} has been completed already.", player);
            return;
        }

        player.Character.Data.CompletedQuests.Add(quest.Id);

        Log($"Added quest {quest.Name} with id {quest.Id} to completed quests list.", player);
    }
}
