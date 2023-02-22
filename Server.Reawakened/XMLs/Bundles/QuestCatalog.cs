using A2m.Server;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using System.Xml;
using UnityEngine;

namespace Server.Reawakened.XMLs.Bundles;

public class QuestCatalog : QuestCatalogXML, IBundledXml
{
    private SortedDictionary<QuestLineDescription, List<QuestDescription>> _questLines;
    private Dictionary<int, QuestDescription> _quests;
    public string BundleName => "QuestCatalog";

    public void InitializeVariables()
    {
        _rootXmlName = BundleName;
        _hasLocalizationDict = false;

        this.SetField<QuestCatalogXML>("QuestLineMap", new Dictionary<TribeType, GameObject>());
        this.SetField<QuestCatalogXML>("_questCatalog", new Dictionary<int, QuestDescription>());
        this.SetField<QuestCatalogXML>("_questLineCatalog", new Dictionary<int, QuestLineDescription>());
        this.SetField<QuestCatalogXML>("_activityQuestLineCatalog", new Dictionary<int, QuestLineDescription>());
        this.SetField<QuestCatalogXML>("_sortedQuestLine", new List<QuestLineGraph>());
        this.SetField<QuestCatalogXML>("_questLines",
            new SortedDictionary<QuestLineDescription, List<QuestDescription>>(new QuestLineSorter()));

        _quests = new Dictionary<int, QuestDescription>();
        _questLines = new SortedDictionary<QuestLineDescription, List<QuestDescription>>();
    }

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml) => ReadDescriptionXml(xml);

    public void FinalizeBundle()
    {
        _quests = this.GetField<QuestCatalogXML>("_questCatalog") as Dictionary<int, QuestDescription>;
        _questLines =
            this.GetField<QuestCatalogXML>("_questLines") as
                SortedDictionary<QuestLineDescription, List<QuestDescription>>;
    }

    public QuestDescription[] GetQuestsBy(int npcId) =>
        _quests.Values.Where(q => q.QuestGiverGoId == npcId).ToArray();

    public List<QuestDescription> GetQuestLineQuests(QuestLineDescription questLine) =>
        _questLines.TryGetValue(questLine, out var v) ? v : null;
}
