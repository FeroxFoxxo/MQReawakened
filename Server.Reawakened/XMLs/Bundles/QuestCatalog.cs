using A2m.Server;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using UnityEngine;

namespace Server.Reawakened.XMLs.Bundles;

public class QuestCatalog : QuestCatalogXML, IBundledXml
{
    public string BundleName => "QuestCatalog";

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
    }
}
