using A2m.Server;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;

public class VendorCatalog : VendorCatalogsXML, IBundledXml
{
    public string BundleName => "vendor_catalogs";

    {
        _rootXmlName = BundleName;
		_hasLocalizationDict = true;

        this.SetField<VendorCatalogsXML>("_levelUpCatalogs", new Dictionary<int, List<int>>());
        this.SetField<VendorCatalogsXML>("_vendorCatalogs", new Dictionary<int, List<int>>());
        this.SetField<VendorCatalogsXML>("_vendorCatalogIds", new Dictionary<string, int>());
        this.SetField<VendorCatalogsXML>("_vendorCatalogIdToVendorId", new Dictionary<int, int>());
        this.SetField<VendorCatalogsXML>("_levelUpCatalogs", new Dictionary<int, List<int>>());
        this.SetField<VendorCatalogsXML>("_cashShops", new Dictionary<string, CashShop>());
        this.SetField<VendorCatalogsXML>("_superPacks", new Dictionary<int, Dictionary<int, int>>());
        this.SetField<VendorCatalogsXML>("_loots", new Dictionary<int, List<LootData>>());
    }

    public void EditXml(XmlDocument xml)
    {
        var items = xml.SelectNodes("/vendor_catalogs/superpacks/superpack/item");

        if (items == null)
            return;

        foreach (XmlNode aNode in items)
        {
            if (aNode.Attributes == null)
                continue;

            var idAttribute = aNode.Attributes["quantity"];

            if (idAttribute != null)
                continue;

            var quantity = xml.CreateAttribute("quantity");
            quantity.Value = "1";

            aNode.Attributes.Append(quantity);
        }
    }

    public void ReadXml(string xml) =>
        ReadDescriptionXml(xml);

    public void FinalizeBundle()
    {
    }
}
