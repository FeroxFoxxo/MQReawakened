using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class InternalIgnoredItem : IBundledXml
{
    public string BundleName => "InternalIgnoredItem";
    public BundlePriority Priority => BundlePriority.Highest;

    private List<string> _ignoredItems;

    public InternalIgnoredItem()
    {
    }

    public void InitializeVariables() =>
        _ignoredItems = [];

    public bool IsItemIgnored(string name) => _ignoredItems.Contains(name.ToUpper());

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

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

    public void FinalizeBundle()
    {
    }
}
