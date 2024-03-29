﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Extensions;
using Server.Reawakened.XMLs.Models.Npcs;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class InternalVendor : IBundledXml<InternalVendor>
{
    public string BundleName => "InternalVendor";
    public BundlePriority Priority => BundlePriority.Low;

    public ILogger<InternalVendor> Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public Dictionary<int, List<VendorInfo>> VendorCatalog;

    public void InitializeVariables() => VendorCatalog = [];

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {
        var miscDict = Services.GetRequiredService<MiscTextDictionary>();
        var itemCat = Services.GetRequiredService<ItemCatalog>();

        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        foreach (XmlNode vendorXml in xmlDocument.ChildNodes)
        {
            if (vendorXml.Name != "VendorCatalog") continue;

            foreach (XmlNode level in vendorXml.ChildNodes)
            {
                if (level.Name != "Level") continue;

                var levelId = -1;

                foreach (XmlAttribute vendorAttribute in level.Attributes)
                    switch (vendorAttribute.Name)
                    {
                        case "id":
                            levelId = int.Parse(vendorAttribute.Value);
                            continue;
                    }

                VendorCatalog.Add(levelId, []);

                foreach (XmlNode vendor in level.ChildNodes)
                {
                    if (vendor.Name != "Vendor") continue;

                    var objectId = -1;
                    var name = string.Empty;
                    var descriptionId = -1;

                    var numberOfIdolsToAccessBackStore = -1;
                    var idolLevelId = -1;

                    var vendorId = -1;
                    var catalogId = -1;
                    var vendorType = NPCController.NPCStatus.Unknown;

                    var dialogId = -1;
                    var greetingConversationId = -1;
                    var leavingConversationId = -1;

                    var items = new List<int>();

                    foreach (XmlAttribute vendorAttribute in vendor.Attributes)
                        switch (vendorAttribute.Name)
                        {
                            case "objectId":
                                objectId = int.Parse(vendorAttribute.Value);
                                continue;
                            case "name":
                                name = vendorAttribute.Value;
                                continue;
                            case "descriptionId":
                                descriptionId = int.Parse(vendorAttribute.Value);
                                continue;

                            case "numberOfIdolsToAccessBackStore":
                                numberOfIdolsToAccessBackStore = int.Parse(vendorAttribute.Value);
                                continue;
                            case "idolLevelId":
                                idolLevelId = int.Parse(vendorAttribute.Value);
                                continue;

                            case "vendorId":
                                vendorId = int.Parse(vendorAttribute.Value);
                                continue;
                            case "catalogId":
                                catalogId = int.Parse(vendorAttribute.Value);
                                continue;
                            case "vendorType":
                                vendorType = vendorType.GetEnumValue(vendorAttribute.Value, Logger);
                                continue;

                            case "dialogId":
                                dialogId = int.Parse(vendorAttribute.Value);
                                continue;
                            case "greetingConversationId":
                                greetingConversationId = int.Parse(vendorAttribute.Value);
                                continue;
                            case "leavingConversationId":
                                leavingConversationId = int.Parse(vendorAttribute.Value);
                                continue;
                        }

                    foreach (XmlNode item in vendor.ChildNodes)
                    {
                        if (!(item.Name == "Item")) continue;

                        foreach (XmlAttribute itemAttribute in item.Attributes)
                            if (itemAttribute.Name == "prefabName")
                            {
                                var itemD = itemCat.GetItemFromPrefabName(itemAttribute.Value);

                                if (itemD == null)
                                {
                                    Logger.LogError("Unknown item with prefab name: {Val}", itemAttribute.Value);
                                    continue;
                                }

                                items.Add(itemD.ItemId);
                                break;
                            }
                    }

                    var nameModel = miscDict.LocalizationDict.FirstOrDefault(x => x.Value == name);

                    if (!string.IsNullOrEmpty(nameModel.Value))
                    {
                        var greetingConversation = new ConversationInfo(dialogId, greetingConversationId);
                        var leavingConversation = new ConversationInfo(dialogId, leavingConversationId);

                        VendorCatalog[levelId].Add(
                            new VendorInfo(
                                objectId, nameModel.Key, descriptionId,
                                numberOfIdolsToAccessBackStore, idolLevelId,
                                vendorId, catalogId, vendorType,
                                greetingConversation, leavingConversation, [.. items]
                            )
                        );
                    }
                    else
                        Logger.LogError("Cannot find text id for character with name: {Name}", name);
                }
            }
        }
    }

    public void FinalizeBundle()
    {
    }

    public VendorInfo GetVendorById(int levelId, int id) =>
        VendorCatalog.TryGetValue(levelId, out var vendors) ? vendors.FirstOrDefault(x => x.GameObjectId == id) : null;
}
