using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Extensions;
using Server.Reawakened.XMLs.Models;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;

public class InternalVendorCatalog : IBundledXml
{
    public string BundleName => "InternalVendorCatalog";
    public BundlePriority Priority => BundlePriority.Low;

    public Microsoft.Extensions.Logging.ILogger Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public Dictionary<int, VendorInfo> VendorCatalog;

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

        foreach (XmlNode vendors in xmlDocument.ChildNodes)
        {
            if (vendors.Name != "VendorCatalog") continue;

            foreach (XmlNode vendor in vendors.ChildNodes)
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

                foreach (XmlAttribute vendorAttributes in vendor.Attributes!)
                {
                    switch (vendorAttributes.Name)
                    {
                        case "objectId":
                            objectId = int.Parse(vendorAttributes.Value);
                            continue;
                        case "name":
                            name = vendorAttributes.Value;
                            continue;
                        case "descriptionId":
                            descriptionId = int.Parse(vendorAttributes.Value);
                            continue;

                        case "numberOfIdolsToAccessBackStore":
                            numberOfIdolsToAccessBackStore = int.Parse(vendorAttributes.Value);
                            continue;
                        case "idolLevelId":
                            idolLevelId = int.Parse(vendorAttributes.Value);
                            continue;

                        case "vendorId":
                            vendorId = int.Parse(vendorAttributes.Value);
                            continue;
                        case "catalogId":
                            catalogId = int.Parse(vendorAttributes.Value);
                            continue;
                        case "vendorType":
                            vendorType = vendorType.GetEnumValue(vendorAttributes.Value, Logger);
                            continue;

                        case "dialogId":
                            dialogId = int.Parse(vendorAttributes.Value);
                            continue;
                        case "greetingConversationId":
                            greetingConversationId = int.Parse(vendorAttributes.Value);
                            continue;
                        case "leavingConversationId":
                            leavingConversationId = int.Parse(vendorAttributes.Value);
                            continue;
                    }
                }

                foreach (XmlNode item in vendor.ChildNodes)
                {
                    if (!(item.Name == "Item")) continue;

                    foreach (XmlAttribute id in item.Attributes)
                    {
                        if (id.Name == "name")
                        {
                            items.Add(itemCat.ItemNameDict[id.Value]);
                            break;
                        }
                    }
                }

                var nameModel = miscDict.LocalizationDict.FirstOrDefault(x => x.Value == name);

                if (!string.IsNullOrEmpty(nameModel.Value))
                {
                    if (VendorCatalog.ContainsKey(objectId))
                        continue;

                    var greetingConversation = new Conversation(dialogId, greetingConversationId);
                    var leavingConversation = new Conversation(dialogId, leavingConversationId);

                    VendorCatalog.Add(objectId, new VendorInfo(
                        objectId, nameModel.Key, descriptionId,
                        numberOfIdolsToAccessBackStore, idolLevelId,
                        vendorId, catalogId, vendorType,
                        greetingConversation, leavingConversation, [.. items]
                    ));
                }
                else
                {
                    Logger.LogError("Cannot find text id for character with name: {Name}", name);
                }
            }
        }
    }

    public void FinalizeBundle()
    {
    }

    public VendorInfo GetVendorById(int id) =>
        VendorCatalog.TryGetValue(id, out var vendor) ? vendor : null;
}
