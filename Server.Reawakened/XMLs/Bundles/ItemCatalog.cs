using A2m.Server;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;
public class ItemCatalog : ItemHandler, ILocalizationXml
{
    public ItemCatalog() : base(null) {}

    public string BundleName => "ItemCatalog";
    public string LocalizationName => "ItemCatalogDict_en-US";

    public void EditXml(XmlDocument xml) { }
    public void FinalizeBundle() { }

    public void InitializeVariables()
    {
        this.SetField<ItemHandler>("_isDisposed", false);
        this.SetField<ItemHandler>("_initDescDone", false);
        this.SetField<ItemHandler>("_initLocDone", false);

        this.SetField<ItemHandler>("_localizationDict", new Dictionary<int, string>());
        this.SetField<ItemHandler>("_itemDescriptionCache", new Dictionary<int, ItemDescription>());
        this.SetField<ItemHandler>("_pendingRequests", new Dictionary<int, ItemDescriptionRequest>());
    }

    public void ReadXml(string xml) => ReadDescriptionXml(xml);

    public void LoadLocalization(string xml) => ReadLocalizationXml(xml);
}
