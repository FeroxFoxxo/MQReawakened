﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Bundles;
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

    // LEVEL ID, OBJECT ID, LOOT MODEL
    public Dictionary<int, Dictionary<string, LootModel>> LootCatalog;

    public void InitializeVariables() => LootCatalog = [];

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {
        var itemCatalog = Services.GetRequiredService<ItemCatalog>();

        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        foreach (XmlNode lootXml in xmlDocument.ChildNodes)
        {
            if (lootXml.Name != "LootCatalog") continue;

            foreach (XmlNode lootLevel in lootXml.ChildNodes)
            {
                if (lootLevel.Name != "Level") continue;

                var lootList = new List<string>();

                var levelId = -1;

                foreach (XmlAttribute vendorAttribute in lootLevel.Attributes)
                    switch (vendorAttribute.Name)
                    {
                        case "id":
                            levelId = int.Parse(vendorAttribute.Value);
                            continue;
                    }

                LootCatalog.Add(levelId, []);

                foreach (XmlNode lootInfo in lootLevel.ChildNodes)
                {
                    if (lootInfo.Name != "LootInfo") continue;

                    var objectId = string.Empty;
                    var doWheel = true;
                    var multiplayerWheelChance = 0;
                    var bananaRewards = new List<BananaReward>();
                    var itemRewards = new List<ItemReward>();
                    var weightRange = 1;

                    foreach (XmlAttribute lootAttribute in lootInfo.Attributes)
                        switch (lootAttribute.Name)
                        {
                            case "objectId":
                                objectId = lootAttribute.Value;
                                continue;
                            case "doLootWheel":
                                doWheel = doWheel.GetBoolValue(lootAttribute.Value, Logger);
                                continue;
                            case "multiplayerWheelChance":
                                multiplayerWheelChance = int.Parse(lootAttribute.Value);
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

                                var itemList = reward.GetXmlLootItems(itemCatalog, Logger);

                                foreach (var item in itemList)
                                    weightRange += item.Key;

                                itemRewards.Add(new ItemReward(itemList, rewardAmount));
                                break;
                            default:
                                Logger.LogWarning("Unknown reward type {RewardType} for object {Id}", lootInfo.Name, objectId);
                                break;
                        }

                    LootCatalog[levelId].Add(objectId, new LootModel(objectId, bananaRewards, itemRewards, doWheel, multiplayerWheelChance, weightRange));
                    lootList.Add(objectId);
                }
            }
        }
    }

    public void FinalizeBundle()
    {
    }

    public LootModel GetLootById(int levelId, string objectId) =>
        LootCatalog.TryGetValue(levelId, out var level) ?
            level.TryGetValue(objectId, out var lootInfo) ?
                lootInfo :
                new LootModel(string.Empty, [], [], false, 0, 1) :
            new LootModel(string.Empty, [], [], false, 0, 1);
}
