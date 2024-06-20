using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;

namespace Protocols.External._n__NpcHandler;
public class AcceptQuest : ExternalProtocol
{
    public override string ProtocolName => "nA";

    public ILogger<ChooseQuestReward> Logger { get; set; }
    public QuestCatalog QuestCatalog { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public FileLogger FileLogger { get; set; }
    public InternalQuestItem QuestItems { get; set; }

    public override void Run(string[] message)
    {
        var quest = int.Parse(message[5]);
        var setActive = message[6] == "1";

        if (Player.Character.ActiveQuestId == quest || Player.Character.CompletedQuests.Contains(quest))
            return;

        var newQuest = QuestCatalog.GetQuestData(quest);

        if (newQuest != null)
            Player.AddQuest(newQuest, QuestItems, ItemCatalog, FileLogger, $"Accept quest protocol", Logger, setActive);
    }
}
