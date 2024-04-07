using Microsoft.Extensions.Logging;
using Server.Reawakened.XMLs.Abstractions.Enums;
using Server.Reawakened.XMLs.Abstractions.Extensions;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Data.Npcs;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles.Internal;

public class InternalVendor : InternalXml
{
    public override string BundleName => "InternalVendor";
    public override BundlePriority Priority => BundlePriority.Low;

    public ILogger<InternalVendor> Logger { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public MiscTextDictionary MiscTextDictionary { get; set; }

    public Dictionary<int, List<VendorInfo>> VendorCatalog;

    public override void InitializeVariables() => VendorCatalog = [];

    public VendorInfo GetVendorById(int levelId, int id) =>
        VendorCatalog.TryGetValue(levelId, out var vendors) ? vendors.FirstOrDefault(x => x.GameObjectId == id) : null;

    public override void ReadDescription(XmlDocument xmlDocument)
    {
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
                                var itemDescription = ItemCatalog.GetItemFromPrefabName(itemAttribute.Value);

                                if (itemDescription == null)
                                {
                                    Logger.LogError("Unknown item with prefab name: '{item}'", itemAttribute.Value);
                                    continue;
                                }

                                if (!ItemCatalog.CanAddItem(itemDescription))
                                    continue;

                                items.Add(itemDescription.ItemId);
                                break;
                            }
                    }

                    var nameModel = MiscTextDictionary.LocalizationDict.FirstOrDefault(x => x.Value == name);

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
                        Logger.LogError("Cannot find text id for character with name: '{Name}'", name);
                }
            }
        }
    }
}
