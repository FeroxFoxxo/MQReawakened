using Server.Base.Core.Extensions;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.XMLs.Abstractions.Enums;
using Server.Reawakened.XMLs.Abstractions.Extensions;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using Server.Reawakened.XMLs.Bundles.Edit;
using Server.Reawakened.XMLs.Bundles.Internal;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles.Base;

public class VendorCatalog : VendorCatalogsXML, IBundledXml
{
    public string BundleName => "vendor_catalogs";
    public BundlePriority Priority => BundlePriority.Lowest;

    public InternalVendor InternalVendor { get; set; }
    public MiscTextDictionary MiscTextDictionary { get; set; }
    public EditVendor EditVendor { get; set; }
    public ServerRConfig ServerRConfig { get; set; }

    public void InitializeVariables()
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

    public void EditDescription(XmlDocument xml)
    {
        var items = xml.SelectNodes("/vendor_catalogs/superpacks/superpack/item");
        var vendors = xml.SelectNodes("/vendor_catalogs/vendor");

        if (items != null)
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

        if (vendors != null)
        {
            var preExistingCategories = new List<int>();

            foreach (XmlNode aNode in vendors)
            {
                if (aNode.Attributes == null)
                    continue;

                var nameAttribute = aNode.Attributes["name"];

                if (nameAttribute != null)
                    if (EditVendor.EditedVendorAttributes[ServerRConfig.GameVersion].TryGetValue(nameAttribute.InnerText, out var lItems))
                        foreach (var item in lItems)
                        {
                            var itemElement = xml.CreateElement("item");
                            itemElement.SetAttribute("id", item);
                            aNode.AppendChild(itemElement);
                        }

                var categoryAttribute = aNode.Attributes["catalogid"];

                if (categoryAttribute != null)
                    preExistingCategories.Add(int.Parse(categoryAttribute.InnerText));
            }

            var lastSmallest = 0;

            foreach (XmlNode vendorCatalogNode in xml.ChildNodes)
            {
                if (!(vendorCatalogNode.Name == "vendor_catalogs")) continue;

                foreach (var vendor in InternalVendor.VendorCatalog.Values.SelectMany(x => x))
                {
                    if (vendor.CatalogId != -1)
                        return;

                    var name = MiscTextDictionary.GetLocalizationTextById(vendor.NameId);
                    var catalogId = preExistingCategories.FindSmallest(lastSmallest);
                    var vendorId = catalogId;

                    lastSmallest = catalogId;

                    vendor.CatalogId = catalogId;
                    vendor.VendorId = vendorId;

                    var vendorElement = xml.CreateElement("vendor");

                    vendorElement.SetAttribute("catalogid", catalogId.ToString());
                    vendorElement.SetAttribute("vendorid", catalogId.ToString());
                    vendorElement.SetAttribute("name", name);

                    foreach (var item in vendor.Items)
                    {
                        var itemElement = xml.CreateElement("item");
                        itemElement.SetAttribute("id", item.ToString());
                        vendorElement.AppendChild(itemElement);
                    }

                    vendorCatalogNode.AppendChild(vendorElement);
                }
            }
        }
    }

    public void ReadDescription(string xml) => ReadDescriptionXml(xml);

    public void FinalizeBundle()
    {
    }
}
