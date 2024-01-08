using Microsoft.Extensions.Logging;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Extensions;
using Server.Reawakened.XMLs.Models.LootRewards;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class LootCatalogInt : IBundledXml
{
    public string BundleName => "LootCatalogInt";
    public BundlePriority Priority => BundlePriority.Low;

    public Microsoft.Extensions.Logging.ILogger Logger { get; set; }
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

        foreach (XmlNode lootCatalog in xmlDocument.ChildNodes)
        {
            if (lootCatalog.Name != "LootCatalog") continue;

            foreach (XmlNode lootLevel in lootCatalog.ChildNodes)
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

                    foreach (XmlAttribute lootAttributes in lootInfo.Attributes)
                        switch (lootAttributes.Name)
                        {
                            case "objectId":
                                objectId = int.Parse(lootAttributes.Value);
                                continue;
                            case "doLootWheel":
                                doWheel = bool.Parse(lootAttributes.Value);
                                continue;
                        }

                    foreach (XmlNode reward in lootInfo.ChildNodes)
                        switch (reward.Name)
                        {
                            case "Bananas":
                                var bananaMin = -1;
                                var bananaMax = -1;

                                foreach (XmlAttribute lootAttributes in reward.Attributes)
                                    switch (lootAttributes.Name)
                                    {
                                        case "bananaMin":
                                            bananaMin = int.Parse(lootAttributes.Value);
                                            continue;
                                        case "bananaMax":
                                            bananaMax = int.Parse(lootAttributes.Value);
                                            continue;
                                    }
                                bananaRewards.Add(new BananaReward(bananaMin, bananaMax));
                                break;
                            case "Items":
                                var rewardAmount = 1;

                                foreach (XmlAttribute lootAttributes in reward.Attributes)
                                    switch (lootAttributes.Name)
                                    {
                                        case "rewardAmount":
                                            bananaMin = int.Parse(lootAttributes.Value);
                                            continue;
                                    }

                                var itemList = reward.GetXmlItems();
                                foreach (var item in itemList)
                                {
                                    weightRange += item.Weight;
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
