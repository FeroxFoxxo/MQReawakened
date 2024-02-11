using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Extensions;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesEdit;
public class EditVendor : IBundledXml<EditVendor>
{
    public string BundleName => "EditVendor";
    public BundlePriority Priority => BundlePriority.Low;

    public ILogger<EditVendor> Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public Dictionary<GameVersion, Dictionary<string, List<string>>> EditedVendorAttributes;

    public EditVendor()
    {
    }

    public void InitializeVariables() =>
        EditedVendorAttributes = [];

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {
        var itemCatalog = Services.GetRequiredService<ItemCatalog>();

        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

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

                        var itemDesc = itemCatalog.GetItemFromPrefabName(prefabName);

                        EditedVendorAttributes[gameVersion][name].Add(itemDesc.ItemId.ToString());
                    }
                }
            }
        }
    }

    public void FinalizeBundle()
    {
    }
}
