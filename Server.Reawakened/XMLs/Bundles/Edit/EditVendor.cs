using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.XMLs.Abstractions.Enums;
using Server.Reawakened.XMLs.Abstractions.Extensions;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using Server.Reawakened.XMLs.Bundles.Base;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles.Edit;
public class EditVendor : InternalXml
{
    public override string BundleName => "EditVendor";
    public override BundlePriority Priority => BundlePriority.Low;

    public ILogger<EditVendor> Logger { get; set; }
    public ItemCatalog ItemCatalog { get; set; }


    public Dictionary<GameVersion, Dictionary<string, List<string>>> EditedVendorAttributes;

    public override void InitializeVariables() =>
        EditedVendorAttributes = [];

    public override void ReadDescription(XmlDocument xmlDocument)
    {
        foreach (XmlNode items in xmlDocument.ChildNodes)
        {
            if (!(items.Name == "EditedVendors")) continue;

            foreach (XmlNode gVXml in items.ChildNodes)
            {
                if (!(gVXml.Name == "GameVersion")) continue;

                var gameVersion = GameVersion.Unknown;

                foreach (XmlAttribute gVAttribute in gVXml.Attributes)
                    switch (gVAttribute.Name)
                    {
                        case "version":
                            gameVersion = gameVersion.GetEnumValue(gVAttribute.Value, Logger);
                            break;
                    }

                EditedVendorAttributes.Add(gameVersion, []);

                foreach (XmlNode item in gVXml.ChildNodes)
                {
                    if (!(item.Name == "Vendor")) continue;

                    var name = string.Empty;
                    var itemId = string.Empty;

                    foreach (XmlAttribute itemAttributes in item.Attributes)
                        switch (itemAttributes.Name)
                        {
                            case "name":
                                name = itemAttributes.Value;
                                break;
                        }

                    EditedVendorAttributes[gameVersion].Add(name, []);

                    foreach (XmlNode itemAttribute in item.ChildNodes)
                    {
                        if (!(itemAttribute.Name == "Item")) continue;

                        var prefabName = string.Empty;

                        foreach (XmlAttribute itemAttributes in itemAttribute.Attributes)
                            switch (itemAttributes.Name)
                            {
                                case "name":
                                    prefabName = itemAttributes.Value;
                                    break;
                            }

                        var itemDesc = ItemCatalog.GetItemFromPrefabName(prefabName);

                        if (itemDesc == null)
                            continue;

                        EditedVendorAttributes[gameVersion][name].Add(itemDesc.ItemId.ToString());
                    }
                }
            }
        }
    }
}
