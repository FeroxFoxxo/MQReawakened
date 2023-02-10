using A2m.Server;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using UnityEngine;

namespace Server.Reawakened.XMLs.Bundles;

public class QuestCatalog : QuestCatalogXML, IBundledXml
{
    public string BundleName => "QuestCatalog";

    private Dictionary<int, QuestDescription> _quests;
    private SortedDictionary<QuestLineDescription, List<QuestDescription>> _questLines;

    public void LoadBundle(string xml)
    {
        _rootXmlName = BundleName;
        _hasLocalizationDict = false;

        QuestLineMap = new Dictionary<TribeType, GameObject>();

        this.SetField<QuestCatalogXML>("_questCatalog", new Dictionary<int, QuestDescription>());
        this.SetField<QuestCatalogXML>("_questLineCatalog", new Dictionary<int, QuestLineDescription>());
        this.SetField<QuestCatalogXML>("_activityQuestLineCatalog", new Dictionary<int, QuestLineDescription>());
        this.SetField<QuestCatalogXML>("_sortedQuestLine", new List<QuestLineGraph>());
        this.SetField<QuestCatalogXML>("_questLines",
            new SortedDictionary<QuestLineDescription, List<QuestDescription>>(new QuestLineSorter()));

        ReadDescriptionXml(xml);

        _quests = this.GetField<QuestCatalogXML>("_questCatalog") as Dictionary<int, QuestDescription>;
        _questLines = this.GetField<QuestCatalogXML>("_questLines") as SortedDictionary<QuestLineDescription, List<QuestDescription>>;
    }

    public List<QuestDescription> GetQuestsBy(int npcId)
    {
        var quests = new List<QuestDescription>();

        foreach(var q in _quests.Values)
        {
            if (q.QuestGiverGoId == npcId)
            {
                quests.Add(q);
            }
        }

        return quests;
    }

    public List<QuestDescription> GetQuestLineQuests(QuestLineDescription questLine) => _questLines.TryGetValue(questLine, out var v) ? v : null;
}
