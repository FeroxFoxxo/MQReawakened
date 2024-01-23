using Microsoft.Extensions.Logging;
using Server.Reawakened.XMLs.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesEdit;
public class EditItem : IBundledXml<EditItem>
{
    public string BundleName => "EditItem";
    public BundlePriority Priority => BundlePriority.Highest;

    public ILogger<EditItem> Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public Dictionary<string, Dictionary<string, string>> EditedItemAttributes;

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

            foreach (XmlNode item in items.ChildNodes)
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

                EditedItemAttributes.Add(name, []);

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

                    EditedItemAttributes[name].Add(key, value);
                }
            }
        }
    }

    public void FinalizeBundle()
    {
    }
}
