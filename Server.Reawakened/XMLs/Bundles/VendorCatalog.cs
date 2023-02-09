using A2m.Server;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using UnityEngine;

namespace Server.Reawakened.XMLs.Bundles;

public class VendorCatalog : VendorCatalogsXML, IBundledXml
{
    public string BundleName => "vendor_catalogs";

    public void LoadBundle(string xml)
    {
        _rootXmlName = BundleName;
		_hasLocalizationDict = true;

        this.SetField<VendorCatalogsXML>("_levelUpCatalogs", new Dictionary<int, List<int>>());
        this.SetField<VendorCatalogsXML>("_vendorCatalogs", new Dictionary<int, List<int>>());
        this.SetField<VendorCatalogsXML>("_vendorCatalogIds", new Dictionary<string, int>());
        this.SetField<VendorCatalogsXML>("_vendorCatalogIdToVendorId", new Dictionary<int, int>());
        this.SetField<VendorCatalogsXML>("_cashShops", new Dictionary<string, CashShop>());
        this.SetField<VendorCatalogsXML>("_superPacks", new Dictionary<int, Dictionary<int, int>>());
        this.SetField<VendorCatalogsXML>("_loots", new Dictionary<int, List<LootData>>());

        ReadDescriptionXml(xml);
    }
}
