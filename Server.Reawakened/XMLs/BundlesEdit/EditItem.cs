using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Extensions;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesEdit;
public class EditItem : IBundledXml<EditItem>
{
    public string BundleName => "EditItem";
    public BundlePriority Priority => BundlePriority.Highest;

    public ILogger<EditItem> Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public Dictionary<GameVersion, Dictionary<string, Dictionary<string, string>>> EditedItemAttributes;

    public EditItem()
    {
    }

    public void InitializeVariables() =>
        EditedItemAttributes = [];

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        foreach (XmlNode items in xmlDocument.ChildNodes)
        {
            if (!(items.Name == "EditedItems")) continue;

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

                EditedItemAttributes.Add(gameVersion, []);

                foreach (XmlNode item in gVXml.ChildNodes)
                {
                    if (!(item.Name == "Item")) continue;

                    var name = string.Empty;
                    var itemId = string.Empty;

                    foreach (XmlAttribute itemAttributes in item.Attributes)
                        switch (itemAttributes.Name)
                        {
                            case "name":
                                name = itemAttributes.Value;
                                break;
                        }

                    EditedItemAttributes[gameVersion].Add(name, []);

                    foreach (XmlNode itemAttribute in item.ChildNodes)
                    {
                        if (!(itemAttribute.Name == "EditAttribute")) continue;

                        var key = string.Empty;
                        var value = string.Empty;

                        foreach (XmlAttribute itemAttributes in itemAttribute.Attributes)
                            switch (itemAttributes.Name)
                            {
                                case "key":
                                    key = itemAttributes.Value;
                                    break;
                                case "value":
                                    value = itemAttributes.Value;
                                    break;
                            }

                        EditedItemAttributes[gameVersion][name].Add(key, value);
                    }
                }
            }
        }
    }

    public void FinalizeBundle()
    {
    }
}
