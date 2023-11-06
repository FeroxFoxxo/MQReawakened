using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;

public class InternalLootCatalog : IBundledXml
{
    public string BundleName => "InternalLootCatalog";
    public BundlePriority Priority => BundlePriority.Low;
    public Microsoft.Extensions.Logging.ILogger Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public Dictionary<int, Dictionary<string, dynamic>> LootCatalog;

    public void InitializeVariables() => LootCatalog = [];

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {

        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        foreach (XmlNode lootCatalog in xmlDocument.ChildNodes)
        {
            if (lootCatalog.Name != "LootCatalog") continue;

            foreach (XmlNode lootInfo in lootCatalog.ChildNodes)
            {
                if (lootInfo.Name != "LootInfo") continue;

                var lootInfoEntry = new Dictionary<string, dynamic>();

                var objectId = "";
                var rewardType = "";
                List<dynamic> reward = [];

                foreach (XmlAttribute lootAttributes in lootInfo.Attributes)
                {
                    switch (lootAttributes.Name)
                    {
                        case "objectId":
                            objectId = lootAttributes.Value;
                            continue;
                        case "rewardType":
                            rewardType = lootAttributes.Value;
                            continue;
                        case "rewardMin":
                            reward.Add(lootAttributes.Value);
                            continue;
                        case "rewardMax":
                            reward.Add(lootAttributes.Value);
                            continue;
                    }
                }

                foreach (XmlNode item in lootInfo.ChildNodes)
                {
                    if (!(item.Name == "Item")) continue;

                    var itemId = -1;
                    var count = -1;
                    var bindingCount = -1;

                    foreach (XmlAttribute itemAttribute in item.Attributes)
                    {
                        switch (itemAttribute.Name)
                        {
                            case "itemId":
                                itemId = int.Parse(itemAttribute.Value);
                                continue;
                            case "count":
                                count = int.Parse(itemAttribute.Value);
                                continue;
                            case "bindingCount":
                                bindingCount = int.Parse(itemAttribute.Value);
                                continue;
                        }
                    }

                    reward.Add(new int[]{itemId, count, bindingCount});
                }

                lootInfoEntry["objectId"] = objectId;
                lootInfoEntry["rewardType"] = rewardType;
                lootInfoEntry["reward"] = reward.ToArray();

                LootCatalog.Add(int.Parse(objectId), lootInfoEntry);
            }
        }
    }

    public void FinalizeBundle()
    {
    }

    public Dictionary<string, object> GetLootById(int objectId)
        => LootCatalog.TryGetValue(objectId, out var lootInfo) ? lootInfo : LootCatalog[0];
}
