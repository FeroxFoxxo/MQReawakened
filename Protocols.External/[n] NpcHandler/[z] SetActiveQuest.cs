using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;

namespace Protocols.External._n__NpcHandler;

public class SetActiveQuest : ExternalProtocol
{
    public override string ProtocolName => "nz";

    public ServerRConfig Config { get; set; }
    public ILogger<ChooseQuestReward> Logger { get; set; }
    public QuestCatalog QuestCatalog { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public FileLogger FileLogger { get; set; }
    public InternalQuestItem QuestItems { get; set; }

    public override void Run(string[] message)
    {
        var activeQuest = int.Parse(message[5]);

        if (activeQuest is -1)
            activeQuest = Config.DefaultQuest;

        if (activeQuest is 0)
        {
            Player.Character.Write.ActiveQuestId = 0;
            return;
        }

        if (Player.Character.ActiveQuestId == activeQuest || Player.Character.CompletedQuests.Contains(activeQuest))
            return;

        var newQuest = QuestCatalog.GetQuestData(activeQuest);

        if (newQuest != null)
            Player.AddQuest(newQuest, QuestItems, ItemCatalog, FileLogger, $"Active quest protocol", Logger);
    }
}
