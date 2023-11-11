﻿using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Extensions;
using Server.Reawakened.XMLs.Models;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;

public class InternalLootCatalog : IBundledXml
{
    public string BundleName => "InternalLootCatalog";
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
                    var bananaMin = -1;
                    var bananaMax = -1;
  
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
                    
                    var itemList = lootInfo.GetXmlItems();
    
                    LootCatalog.TryAdd(objectId, new LootModel(objectId, bananaMin, bananaMax, itemList));
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