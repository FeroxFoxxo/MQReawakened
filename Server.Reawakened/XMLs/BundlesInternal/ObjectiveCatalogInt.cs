using Microsoft.Extensions.DependencyInjection;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Models.Quests;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class ObjectiveCatalogInt : IBundledXml
{
    public string BundleName => "ObjectiveCatalogInt";
    public BundlePriority Priority => BundlePriority.Lowest;

    public Microsoft.Extensions.Logging.ILogger Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public Dictionary<string, ObjectiveInternal> ObjectivePrefabs;

    public ObjectiveCatalogInt()
    {
    }

    public void InitializeVariables() =>
        ObjectivePrefabs = [];

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        foreach (XmlNode quests in xmlDocument.ChildNodes)
        {
            if (!(quests.Name == "ObjectiveCatalogs")) continue;

            foreach (XmlNode quest in quests.ChildNodes)
            {
                if (!(quest.Name == "Objective")) continue;

                var prefabName = string.Empty;
                var itemId = string.Empty;

                foreach (XmlAttribute itemAttributes in quest.Attributes)
                    switch (itemAttributes.Name)
                    {
                        case "prefabName":
                            prefabName = itemAttributes.Value;
                            break;
                        case "itemId":
                            itemId = itemAttributes.Value;
                            break;
                    }
                AddItem(prefabName, itemId, false);
            }
        }

        var itemCatalog = Services.GetRequiredService<ItemCatalog>();

        foreach (var item in itemCatalog.Items.Values)
            AddItem(item.PrefabName, item.ItemId.ToString(), true);
    }

    public void AddItem(string prefabName, string item, bool globalLevel)
    {
        if (!ObjectivePrefabs.ContainsKey(prefabName))
            ObjectivePrefabs.Add(prefabName, new ObjectiveInternal() { ItemIds = [], GlobalLevel = globalLevel });

        var itemId = int.TryParse(item, out var id) ? id : default;

        if (!ObjectivePrefabs[prefabName].ItemIds.Contains(itemId))
            ObjectivePrefabs[prefabName].ItemIds.Add(itemId);
    }

    public void FinalizeBundle()
    {
    }
}
