using A2m.Server;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;

public class ItemCatalog : ItemHandler, ILocalizationXml
{
    public ItemCatalog() : base(null)
    {
    }

    public string BundleName => "ItemCatalog";
    public string LocalizationName => "ItemCatalogDict_en-US";

    public void InitializeVariables()
    {
        this.SetField<ItemHandler>("_isDisposed", false);
        this.SetField<ItemHandler>("_initDescDone", false);
        this.SetField<ItemHandler>("_initLocDone", false);

        this.SetField<ItemHandler>("_localizationDict", new Dictionary<int, string>());
        this.SetField<ItemHandler>("_itemDescriptionCache", new Dictionary<int, ItemDescription>());
        this.SetField<ItemHandler>("_pendingRequests", new Dictionary<int, ItemDescriptionRequest>());
    }

    public void EditLocalization(XmlDocument xml)
    {
    }

    public void ReadLocalization(string xml) => ReadLocalizationXml(xml);

    public void EditDescription(XmlDocument xml, IServiceProvider services)
    {
    }

    public void ReadDescription(string xml) => ReadDescriptionXml(xml);

    public void FinalizeBundle()
    {
    }
}
