using A2m.Server;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions.Enums;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using System.Xml;
using UnityEngine;

namespace Server.Reawakened.XMLs.Bundles.Base;

public class QuestCatalog : QuestCatalogXML, IBundledXml
{
    public string BundleName => "QuestCatalog";
    public BundlePriority Priority => BundlePriority.Low;

    public Dictionary<int, QuestDescription> QuestCatalogs;
    public Dictionary<int, QuestLineDescription> QuestLineCatalogs;

    public ItemCatalog ItemCatalog { get; set; }

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

        QuestCatalogs = [];
        QuestLineCatalogs = [];
    }

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml) => ReadDescriptionXml(xml);

    public void FinalizeBundle()
    {
        GameFlow.QuestCatalog = this;

        QuestCatalogs = this.GetField<QuestCatalogXML>("_questCatalog") as Dictionary<int, QuestDescription>;
        QuestLineCatalogs = this.GetField<QuestCatalogXML>("_questLineCatalog") as Dictionary<int, QuestLineDescription>;
    }

    public QuestDescription[] GetQuestGiverById(int npcId) =>
        [.. QuestCatalogs.Values.Where(q => q.QuestGiverGoId == npcId)];

    public QuestDescription[] GetQuestGiverByName(string npcName) =>
        [.. QuestCatalogs.Values.Where(q => q.QuestgGiverName == npcName)];

    public QuestDescription[] GetQuestValidatorById(int npcId) =>
        [.. QuestCatalogs.Values.Where(q => q.ValidatorGoId == npcId)];

    public List<QuestDescription> GetQuestLineQuests(QuestLineDescription questLine) =>
        QuestLines.TryGetValue(questLine, out var v) ? v : null;
}
