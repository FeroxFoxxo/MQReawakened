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
    public BundlePriority Priority => BundlePriority.Medium;

    public Microsoft.Extensions.Logging.ILogger Logger { get; set; }
    public IServiceProvider Services { get; set; }

    private int _smallestItemDictId;
    private List<int> _itemDictCount;
    private Dictionary<ItemCategory, XmlNode> _itemCategories;
    private Dictionary<ItemCategory, Dictionary<ItemSubCategory, XmlNode>> _itemSubCategories;

    public Dictionary<int, ItemDescription> Items;
    public Dictionary<string, int> ItemNameDict;

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

        ItemNameDict = [];
        _itemDictCount = [];
        _itemCategories = [];
        _itemSubCategories = [];

        Items = [];
    }

    public void EditLocalization(XmlDocument xml)
    {
        ItemNameDict.Clear();
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

                var local = int.Parse(idAttribute.InnerText);

                _itemDictCount.Add(local);
                ItemNameDict.TryAdd(aNode.InnerText, local);
            }

            _smallestItemDictId = 0;

            foreach (XmlNode itemCatalogNode in xml.ChildNodes)
            {
                if (!(itemCatalogNode.Name == "ItemCatalogDict")) continue;

                foreach (var item in internalCatalog.Items)
                {
                    ItemNameDict.Add(item.ItemName, AddDictIfNotExists(xml, itemCatalogNode, item.ItemName, localization));
                    ItemNameDict.Add(item.DescriptionText, AddDictIfNotExists(xml, itemCatalogNode, item.DescriptionText, localization));
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
        _itemCategories.Clear();
        _itemSubCategories.Clear();

        var itemIds = new List<int>();

        foreach (XmlNode catalogs in xml.ChildNodes)
        {
            if (!(catalogs.Name == "Catalog")) continue;

            foreach (XmlNode category in catalogs.ChildNodes)
            {
                if (!(category.Name == "ItemCategory")) continue;

                var itemCategory = ItemCategory.Unknown;

                foreach (XmlAttribute categoryAttributes in category.Attributes)
                    if (categoryAttributes.Name == "id")
                        itemCategory = (ItemCategory)int.Parse(categoryAttributes.Value);

                _itemCategories.TryAdd(itemCategory, category);
                _itemSubCategories.TryAdd(itemCategory, new Dictionary<ItemSubCategory, XmlNode>());

                foreach (XmlNode subCategories in category.ChildNodes)
                {
                    if (!(subCategories.Name == "ItemSubcategory")) continue;

                    var subCategory = ItemSubCategory.Unknown;

                    foreach (XmlAttribute subCategoryAttributes in subCategories.Attributes)
                        if (subCategoryAttributes.Name == "id")
                            subCategory = (ItemSubCategory)int.Parse(subCategoryAttributes.Value);

                    _itemSubCategories[itemCategory].TryAdd(subCategory, subCategories);

                    foreach (XmlNode item in subCategories.ChildNodes)
                    {
                        if (!(item.Name == "Item")) continue;

                        foreach (XmlAttribute itemAttributes in item.Attributes)
                        {
                            switch (itemAttributes.Name)
                            {
                                case "id":
                                    itemIds.Add(int.Parse(itemAttributes.Value));
                                    break;
                            }
                        }
                    }
                }
            }

            var internalCatalog = Services.GetRequiredService<InternalItemCatalog>();
            var smallestItemId = 0;

            foreach (var item in internalCatalog.Items)
            {
                if (!_itemCategories.ContainsKey(item.CategoryId))
                {
                    var category = xml.CreateElement("ItemCategory");

                    category.SetAttribute("id", ((int) item.CategoryId).ToString());
                    category.SetAttribute("name", Enum.GetName(item.CategoryId));

                    var node = catalogs.AppendChild(category);
                    _itemCategories.Add(item.CategoryId, node);
                    _itemSubCategories.TryAdd(item.CategoryId, new Dictionary<ItemSubCategory, XmlNode>());
                }

                var itemCategory = _itemCategories[item.CategoryId];

                if (!_itemSubCategories[item.CategoryId].ContainsKey(item.SubCategoryId))
                {
                    var subCategory = xml.CreateElement("ItemSubcategory");

                    subCategory.SetAttribute("id", ((int) item.SubCategoryId).ToString());
                    subCategory.SetAttribute("name", Enum.GetName(item.SubCategoryId));

                    var node = itemCategory.AppendChild(subCategory);
                    _itemSubCategories[item.CategoryId].Add(item.SubCategoryId, node);
                }

                var itemSubCategory = _itemSubCategories[item.CategoryId][item.SubCategoryId];

                var itemElement = xml.CreateElement("Item");

                var storeType = string.Empty;

                if (item.Store == StoreType.FrontStore)
                    storeType = "Front Store";
                else if (item.Store == StoreType.BackStore)
                    storeType = "Back Store";

                var itemId = itemIds.FindSmallest(smallestItemId);
                itemIds.Add(smallestItemId);
                smallestItemId = itemId;

                itemElement.SetAttribute("id", itemId.ToString());
                itemElement.SetAttribute("ingamename", ItemNameDict[item.ItemName].ToString());
                itemElement.SetAttribute("ingamedescription", ItemNameDict[item.DescriptionText].ToString());
                itemElement.SetAttribute("prefab", item.PrefabName.ToString());
                itemElement.SetAttribute("special_display_prefab", item.SpecialDisplayPrefab.ToString());

                itemElement.SetAttribute("tribe", Enum.GetName(item.Tribe));
                itemElement.SetAttribute("action_type", Enum.GetName(item.ItemActionType));
                itemElement.SetAttribute("rarity", Enum.GetName(item.Rarity));
                itemElement.SetAttribute("currency", Enum.GetName(item.Currency)); itemElement.SetAttribute("member_only", item.MemberOnly ? "true" : "false");
                itemElement.SetAttribute("member_only", item.MemberOnly ? "true" : "false");

                itemElement.SetAttribute("item_level", item.Level.ToString());
                itemElement.SetAttribute("global_level", item.LevelRequired.ToString());
                itemElement.SetAttribute("unique_in_inventory", item.UniqueInInventory ? "true" : "false");

                itemElement.SetAttribute("store_type", storeType);
                itemElement.SetAttribute("stock_priority", item.StockPriority.ToString());
                itemElement.SetAttribute("price", item.RegularPrice.ToString());
                itemElement.SetAttribute("price_discount", item.DiscountPrice.ToString());
                itemElement.SetAttribute("sell_price", item.SellPrice.ToString());
                itemElement.SetAttribute("sell_count", item.SellCount.ToString());
                itemElement.SetAttribute("discounted_from", item.DiscountedFrom == new DateTime(0L) ? "None" : item.DiscountedFrom.ToString());
                itemElement.SetAttribute("discounted_to", item.DiscountedTo == new DateTime(0L) ? "None" : item.DiscountedTo.ToString());

                itemElement.SetAttribute("cooldown_time", item.CooldownTime.ToString());
                itemElement.SetAttribute("delay_use_duration", item.DelayUseDuration.ToString());
                itemElement.SetAttribute("bind_type", Enum.GetName(item.Binding));

                itemElement.SetAttribute("recipe_parent_item_id", item.RecipeParentItemID.ToString());
                itemElement.SetAttribute("loot_id", item.LootId.ToString());

                itemElement.SetAttribute("production_status", Enum.GetName(item.ProductionStatus));
                itemElement.SetAttribute("release_date", item.ReleaseDate == DateTime.UnixEpoch ? "None" : item.ReleaseDate.ToString());

                itemSubCategory.AppendChild(itemElement);
            }
        }
    }

    public void ReadDescription(string xml) =>
        ReadDescriptionXml(xml);

    public void FinalizeBundle() =>
        Items = (Dictionary<int, ItemDescription>) this.GetField<ItemHandler>("_itemDescriptionCache");
}
