using A2m.Server;
using Microsoft.Extensions.DependencyInjection;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Extensions;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;

public class ItemCatalog : ItemHandler, ILocalizationXml
{
    public string BundleName => "ItemCatalog";
    public string LocalizationName => "ItemCatalogDict_en-US";
    public BundlePriority Priority => BundlePriority.Low;

    public Microsoft.Extensions.Logging.ILogger Logger { get; set; }
    public IServiceProvider Services { get; set; }

    private int _smallestItemDictId;
    private List<int> _itemDictCount;
    private Dictionary<string, int> _newItemDict;

    public ItemCatalog() : base(null)
    {
    }

    public void InitializeVariables()
    {
        this.SetField<ItemHandler>("_isDisposed", false);
        this.SetField<ItemHandler>("_initDescDone", false);
        this.SetField<ItemHandler>("_initLocDone", false);

        this.SetField<ItemHandler>("_localizationDict", new Dictionary<int, string>());
        this.SetField<ItemHandler>("_itemDescriptionCache", new Dictionary<int, ItemDescription>());
        this.SetField<ItemHandler>("_pendingRequests", new Dictionary<int, ItemDescriptionRequest>());

        _newItemDict = [];
        _itemDictCount = [];
    }

    public void EditLocalization(XmlDocument xml)
    {
        _newItemDict.Clear();
        _itemDictCount.Clear();

        var dicts = xml.SelectNodes("/ItemCatalogDict/text");

        if (dicts != null)
        {
            var internalCatalog = Services.GetRequiredService<InternalItemCatalog>();

            ReadLocalizationXml(xml.WriteToString());
            var localization = this.GetField<ItemHandler>("_localizationDict") as Dictionary<int, string>;

            foreach (XmlNode aNode in dicts)
            {
                if (aNode.Attributes == null)
                    continue;

                var idAttribute = aNode.Attributes["id"];

                if (idAttribute == null)
                    continue;

                _itemDictCount.Add(int.Parse(idAttribute.InnerText));
            }

            _smallestItemDictId = 0;

            foreach (XmlNode itemCatalogNode in xml.ChildNodes)
            {
                if (!(itemCatalogNode.Name == "ItemCatalogDict")) continue;

                foreach (var item in internalCatalog.Items)
                {
                    _newItemDict.Add(item.ItemName, AddDictIfNotExists(xml, itemCatalogNode, item.ItemName, localization));
                    _newItemDict.Add(item.DescriptionText, AddDictIfNotExists(xml, itemCatalogNode, item.DescriptionText, localization));
                }
            }
        }

        _itemDictCount.Clear();
    }

    private int AddDictIfNotExists(XmlDocument xml, XmlNode node, string text, Dictionary<int, string> dictList)
    {
        var tryGetDict = dictList.FirstOrDefault(x => x.Value == text);

        if (!string.IsNullOrEmpty(tryGetDict.Value))
            return tryGetDict.Key;

        var nameId = _itemDictCount.FindSmallest(_smallestItemDictId);

        var vendorElement = xml.CreateElement("text");

        vendorElement.SetAttribute("id", nameId.ToString());
        vendorElement.InnerText = text;

        _smallestItemDictId = nameId;
        _itemDictCount.Add(nameId);

        node.AppendChild(vendorElement);

        return nameId;
    }

    public void ReadLocalization(string xml) =>
        ReadLocalizationXml(xml.ToString());

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml) =>
        ReadDescriptionXml(xml);

    public void FinalizeBundle()
    {
    }
}
