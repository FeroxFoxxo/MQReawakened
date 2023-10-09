using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;
using static Analytics;

namespace Protocols.External._n__NpcHandler;

public class SetActiveQuest : ExternalProtocol
{
    public override string ProtocolName => "nz";

    public ServerRConfig Config { get; set; }
    public QuestCatalog QuestCatalog { get; set; }

    public override void Run(string[] message)
    {
        var character = Player.Character;

        if (character == null)
            return;

        var activeQuest = int.Parse(message[5]);

        if (activeQuest == -1)
            activeQuest = Config.DefaultQuest;

        if (character.Data.ActiveQuestId == activeQuest || character.Data.CompletedQuests.Contains(activeQuest))
            return;

        var questData = QuestCatalog.GetQuestData(activeQuest);

        Player.AddQuest(questData, true);

        if (character.TryGetQuest(activeQuest, out var quest))
            quest.QuestStatus = QuestStatus.QuestState.IN_PROCESSING;

        Player.UpdateNpcsInLevel(questData);
    }
}
