using Server.Reawakened.XMLs.Abstractions.Enums;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles.Internal;

public class InternalIgnoredItem : InternalXml
{
    public override string BundleName => "InternalIgnoredItem";
    public override BundlePriority Priority => BundlePriority.Highest;

    private List<string> _ignoredItems;

    public override void InitializeVariables() =>
        _ignoredItems = [];

    public bool IsItemIgnored(string name) => _ignoredItems.Contains(name.ToUpper());

    public override void ReadDescription(XmlDocument xmlDocument)
    {
        foreach (XmlNode itemsXml in xmlDocument.ChildNodes)
        {
            if (!(itemsXml.Name == "IgnoredItems")) continue;

            foreach (XmlNode itemXml in itemsXml.ChildNodes)
            {
                if (!(itemXml.Name == "Item")) continue;

                var itemName = string.Empty;

                foreach (XmlAttribute gVAttribute in itemXml.Attributes)
                    switch (gVAttribute.Name)
                    {
                        case "itemName":
                            itemName = gVAttribute.Value;
                            break;
                    }

                itemName = itemName.ToUpper();

                if (!_ignoredItems.Contains(itemName) && !string.IsNullOrEmpty(itemName))
                    _ignoredItems.Add(itemName);
            }
        }
    }
}
