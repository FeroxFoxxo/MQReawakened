using A2m.Server;
using Microsoft.Extensions.DependencyInjection;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class ObjectiveCatalogInt : IBundledXml
{
    public string BundleName => "ObjectiveCatalogInt";
    public BundlePriority Priority => BundlePriority.Lowest;

    public Microsoft.Extensions.Logging.ILogger Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public Dictionary<string, List<string>> ObjectivePrefabs;

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
                AddItem(prefabName, itemId);
            }
        }

        var itemCatalog = Services.GetRequiredService<ItemCatalog>();

        foreach (var item in itemCatalog.Items.Values)
            AddItem(item.PrefabName, item.ItemId.ToString());
    }

    public void AddItem(string prefabName, string itemId)
    {
        if (!ObjectivePrefabs.ContainsKey(prefabName))
            ObjectivePrefabs.Add(prefabName, []);

        if (!ObjectivePrefabs[prefabName].Contains(itemId))
            ObjectivePrefabs[prefabName].Add(itemId);
    }

    public void FinalizeBundle()
    {
    }
}
