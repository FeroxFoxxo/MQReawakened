using A2m.Server;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Extensions;
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

        this.SetPrivateField<QuestCatalogXML>("_questCatalog", new Dictionary<int, QuestDescription>());
        this.SetPrivateField<QuestCatalogXML>("_questLineCatalog", new Dictionary<int, QuestLineDescription>());
        this.SetPrivateField<QuestCatalogXML>("_activityQuestLineCatalog", new Dictionary<int, QuestLineDescription>());
        this.SetPrivateField<QuestCatalogXML>("_sortedQuestLine", new List<QuestLineGraph>());
        this.SetPrivateField<QuestCatalogXML>("_questLines",
            new SortedDictionary<QuestLineDescription, List<QuestDescription>>(new QuestLineSorter()));

        ReadDescriptionXml(xml);
    }
}
