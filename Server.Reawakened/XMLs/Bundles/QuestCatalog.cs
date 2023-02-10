using A2m.Server;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using System.Xml;
using UnityEngine;

namespace Server.Reawakened.XMLs.Bundles;

public class QuestCatalog : QuestCatalogXML, IBundledXml
{
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
    }

    public void EditXml(XmlDocument xml)
    {
    }

    public void ReadXml(string xml) =>
        ReadDescriptionXml(xml);
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
