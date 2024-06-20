using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Enums;
using Server.Base.Logging;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Quest;
public class AddQuest : SlashCommand
{
    public override string CommandName => "/AddQuest";

    public override string CommandDescription => "Add the provided quest by id.";

    public override List<ParameterModel> Parameters =>
    [
        new ParameterModel()
        {
            Name = "id",
            Description = "The provided quest id.",
            Optional = false
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Player;

    public QuestCatalog QuestCatalog { get; set; }
    public InternalQuestItem InternalQuestItem { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public FileLogger FileLogger { get; set; }
    public ILogger<AddQuest> Logger { get; set; }

    public override void Execute(Player player, string[] args)
    {
        var questData = GetQuest(player, args);

        if (questData == null)
            return;

        var questModel = player.Character.QuestLog.FirstOrDefault(x => x.Id == questData.Id);

        if (questModel != null)
        {
            Log("Quest is already in progress.", player);
            return;
        }

        player.AddQuest(questData, InternalQuestItem, ItemCatalog, FileLogger, "Slash command", Logger);
        Log($"Added quest {questData.Name} with id {questData.Id}.", player);
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
