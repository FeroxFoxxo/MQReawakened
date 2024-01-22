using Microsoft.Extensions.Logging;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Extensions;
using Server.Reawakened.XMLs.Models.LootRewards;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class InternalLoot : IBundledXml<InternalLoot>
{
    public string BundleName => "InternalLoot";
    public BundlePriority Priority => BundlePriority.Low;

    public ILogger<InternalLoot> Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public Dictionary<int, LootModel> LootCatalog;

    public void InitializeVariables() => LootCatalog = [];

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        foreach (XmlNode lootXml in xmlDocument.ChildNodes)
        {
            if (lootXml.Name != "LootCatalog") continue;

            foreach (XmlNode lootLevel in lootXml.ChildNodes)
            {
                if (lootLevel.Name != "Level") continue;

                foreach (XmlNode lootInfo in lootLevel.ChildNodes)
                {
                    if (lootInfo.Name != "LootInfo") continue;

                    var objectId = -1;
                    var doWheel = true;
                    var bananaRewards = new List<BananaReward>();
                    var itemRewards = new List<ItemReward>();
                    var weightRange = 1;

                    foreach (XmlAttribute lootAttribute in lootInfo.Attributes)
                        switch (lootAttribute.Name)
                        {
                            case "objectId":
                                objectId = int.Parse(lootAttribute.Value);
                                continue;
                            case "doLootWheel":
                                doWheel = doWheel.GetBoolValue(lootAttribute.Value, Logger);
                                continue;
                        }

                    foreach (XmlNode reward in lootInfo.ChildNodes)
                        switch (reward.Name)
                        {
                            case "Bananas":
                                var bananaMin = -1;
                                var bananaMax = -1;

                                foreach (XmlAttribute rewardAttribute in reward.Attributes)
                                    switch (rewardAttribute.Name)
                                    {
                                        case "bananaMin":
                                            bananaMin = int.Parse(rewardAttribute.Value);
                                            continue;
                                        case "bananaMax":
                                            bananaMax = int.Parse(rewardAttribute.Value);
                                            continue;
                                    }
                                bananaRewards.Add(new BananaReward(bananaMin, bananaMax));
                                break;
                            case "Items":
                                var rewardAmount = 1;

                                foreach (XmlAttribute rewardAttribute in reward.Attributes)
                                    switch (rewardAttribute.Name)
                                    {
                                        case "rewardAmount":
                                            bananaMin = int.Parse(rewardAttribute.Value);
                                            continue;
                                    }

                                var itemList = reward.GetXmlLootItems();

                                foreach (var item in itemList)
                                {
                                    weightRange += item.Key;
                                }

                                itemRewards.Add(new ItemReward(itemList, rewardAmount));
                                break;
                            default:
                                Logger.LogWarning("Unknown reward type {RewardType} for object {Id}", lootInfo.Name, objectId);
                                break;
                        }

                    LootCatalog.TryAdd(objectId, new LootModel(objectId, bananaRewards, itemRewards, doWheel, weightRange));
                }
            }
        }
    }

    public void FinalizeBundle()
    {
    }

    public LootModel GetLootById(int objectId) =>
        LootCatalog.TryGetValue(objectId, out var lootInfo) ? lootInfo : LootCatalog[0];
}
