using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Models.Quests;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class InternalObjective : IBundledXml<InternalObjective>
{
    public string BundleName => "InternalObjective";
    public BundlePriority Priority => BundlePriority.Lowest;

    public ILogger<InternalObjective> Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public Dictionary<string, ObjectiveInternal> ObjectivePrefabs;

    public InternalObjective()
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

        foreach (XmlNode objectiveXml in xmlDocument.ChildNodes)
        {
            if (!(objectiveXml.Name == "ObjectiveCatalogs")) continue;

            foreach (XmlNode objective in objectiveXml.ChildNodes)
            {
                if (!(objective.Name == "Objective")) continue;

                var prefabName = string.Empty;
                var itemId = string.Empty;

                foreach (XmlAttribute objectiveAttribute in objective.Attributes)
                    switch (objectiveAttribute.Name)
                    {
                        case "prefabName":
                            prefabName = objectiveAttribute.Value;
                            break;
                        case "itemId":
                            itemId = objectiveAttribute.Value;
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
