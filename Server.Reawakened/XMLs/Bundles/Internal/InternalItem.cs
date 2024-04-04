using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesEdit;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Extensions;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class InternalItem : InternalXml
{
    public override string BundleName => "InternalItem";
    public override BundlePriority Priority => BundlePriority.High;

    public ILogger<InternalItem> Logger { get; set; }
    public MiscTextDictionary MiscTextDictionary { get; set; }
    public EditItem EditItem { get; set; }
    public InternalObjective InternalObjective { get; set; }

    public Dictionary<int, ItemDescription> Items;
    public Dictionary<int, string> Descriptions;

    public override void InitializeVariables()
    {
        Items = [];
        Descriptions = [];
    }

    public override void ReadDescription(XmlDocument xmlDocument)
    {
        foreach (XmlNode itemXml in xmlDocument.ChildNodes)
        {
            if (!(itemXml.Name == "Catalog")) continue;

            foreach (XmlNode itemCategory in itemXml.ChildNodes)
            {
                if (!(itemCategory.Name == "ItemCategory")) continue;

                var itemCategoryType = ItemCategory.Unknown;

                foreach (XmlAttribute categoryAttribute in itemCategory.Attributes)
                    if (categoryAttribute.Name == "name")
                        itemCategoryType = itemCategoryType.GetEnumValue(categoryAttribute.Value, Logger);

                foreach (XmlNode subCategory in itemCategory.ChildNodes)
                {
                    if (!(subCategory.Name == "ItemSubCategory")) continue;

                    var subCategoryType = ItemSubCategory.Unknown;

                    foreach (XmlAttribute subCategoryAttribute in subCategory.Attributes)
                        if (subCategoryAttribute.Name == "name")
                            subCategoryType = subCategoryType.GetEnumValue(subCategoryAttribute.Value, Logger);

                    foreach (XmlNode item in subCategory.ChildNodes)
                    {
                        if (!(item.Name == "Item")) continue;

                        var itemId = -1;
                        var itemName = string.Empty;
                        var descriptionId = 0;
                        var prefabName = string.Empty;
                        var specialDisplayPrefab = string.Empty;
                        var tribe = TribeType.Unknown;
                        var rarity = ItemRarity.Unknown;
                        var memberOnly = false;

                        var currency = CurrencyType.Unknown;
                        var storeType = StoreType.Invalid;
                        var stockPriority = 0;
                        var regularPrice = 0;
                        var discountPrice = 0;
                        var sellPrice = 0;
                        var sellCount = 0;
                        var discountedFrom = new DateTime(0L);
                        var discountedTo = new DateTime(0L);

                        var actionType = ItemActionType.None;
                        var cooldownTime = 0f;
                        var delayUseDuration = 0;
                        var binding = ItemBinding.Unknown;

                        var level = -1;
                        var levelRequirement = -1;

                        var itemEffects = new List<ItemEffect>();

                        var uniqueInInventory = false;

                        var lootId = -1;
                        var recipeParentItemId = -1;

                        var productionStatus = ProductionStatus.Unknown;
                        var releaseDate = DateTime.UnixEpoch;

                        foreach (XmlAttribute itemAttribute in item.Attributes)
                            switch (itemAttribute.Name)
                            {
                                case "itemId":
                                    itemId = int.Parse(itemAttribute.Value);
                                    break;

                                case "itemName":
                                    itemName = itemAttribute.Value;
                                    break;
                                case "descriptionId":
                                    descriptionId = int.Parse(itemAttribute.Value);
                                    break;
                                case "prefabName":
                                    prefabName = itemAttribute.Value;
                                    break;
                                case "specialDisplayPrefab":
                                    specialDisplayPrefab = itemAttribute.Value;
                                    break;
                                case "tribe":
                                    tribe = tribe.GetEnumValue(itemAttribute.Value, Logger);
                                    break;
                                case "rarity":
                                    rarity = rarity.GetEnumValue(itemAttribute.Value, Logger);
                                    break;
                                case "memberOnly":
                                    memberOnly = memberOnly.GetBoolValue(itemAttribute.Value, Logger);
                                    break;

                                case "currency":
                                    currency = currency.GetEnumValue(itemAttribute.Value, Logger);
                                    break;
                                case "regularPrice":
                                    regularPrice = int.Parse(itemAttribute.Value);
                                    break;
                                case "sellPrice":
                                    sellPrice = int.Parse(itemAttribute.Value);
                                    break;
                                case "sellCount":
                                    sellCount = int.Parse(itemAttribute.Value);
                                    break;

                                case "actionType":
                                    actionType = actionType.GetEnumValue(itemAttribute.Value, Logger);
                                    break;
                                case "cooldownTime":
                                    cooldownTime = float.Parse(itemAttribute.Value);
                                    break;
                                case "delayUseDuration":
                                    delayUseDuration = int.Parse(itemAttribute.Value);
                                    break;
                                case "binding":
                                    binding = binding.GetEnumValue(itemAttribute.Value, Logger);
                                    break;

                                case "level":
                                    level = int.Parse(itemAttribute.Value);
                                    break;
                                case "levelRequirement":
                                    levelRequirement = int.Parse(itemAttribute.Value);
                                    break;

                                case "uniqueInInventory":
                                    uniqueInInventory = uniqueInInventory.GetBoolValue(itemAttribute.Value, Logger);
                                    break;

                                case "storeType":
                                    storeType = storeType.GetEnumValue(itemAttribute.Value, Logger);
                                    break;
                                case "discountedFrom":
                                    discountedFrom = discountedFrom.GetDateValue(itemAttribute.Value, Logger);
                                    break;
                                case "discountedTo":
                                    discountedTo = discountedTo.GetDateValue(itemAttribute.Value, Logger);
                                    break;
                                case "discountPrice":
                                    discountPrice = int.Parse(itemAttribute.Value);
                                    break;
                                case "stockPriority":
                                    stockPriority = int.Parse(itemAttribute.Value);
                                    break;

                                case "lootId":
                                    lootId = int.Parse(itemAttribute.Value);
                                    break;
                                case "recipeParentItemId":
                                    recipeParentItemId = int.Parse(itemAttribute.Value);
                                    break;

                                case "productionStatus":
                                    productionStatus = productionStatus.GetEnumValue(itemAttribute.Value, Logger);
                                    break;
                                case "releaseDate":
                                    releaseDate = releaseDate.GetDateValue(itemAttribute.Value, Logger);
                                    break;
                            }

                        foreach (XmlNode itemEffect in item.ChildNodes)
                        {
                            if (itemEffect.Name != "ItemEffects") continue;

                            foreach (XmlNode effect in itemEffect.ChildNodes)
                            {
                                if (effect.Name != "Effect") continue;

                                var type = ItemEffectType.Unknown;
                                var value = -1;
                                var duration = -1;

                                foreach (XmlAttribute effectAttribute in effect.Attributes)
                                    switch (effectAttribute.Name)
                                    {
                                        case "type":
                                            type = type.GetEnumValue(effectAttribute.Value, Logger);
                                            break;
                                        case "value":
                                            value = int.Parse(effectAttribute.Value);
                                            break;
                                        case "duration":
                                            duration = int.Parse(effectAttribute.Value);
                                            break;
                                    }
                                itemEffects.Add(new ItemEffect(type, value, duration));
                            }
                        }

                        var description = string.Empty;

                        if (MiscTextDictionary.LocalizationDict.TryGetValue(descriptionId, out var miscDescription))
                        {
                            description = miscDescription;
                            Descriptions.TryAdd(descriptionId, description);
                        }
                        else
                        {
                            var attributes = EditItem.GetItemAttributes(prefabName);

                            if (attributes.TryGetValue("ingamedescription", out var attributeValue))
                            {
                                var editedDescriptionId = int.Parse(attributeValue);

                                if (MiscTextDictionary.LocalizationDict.TryGetValue(editedDescriptionId, out var editedDescription))
                                {
                                    Descriptions.TryAdd(editedDescriptionId, editedDescription);
                                    break;
                                }
                            }
                        }

                        var nameId = MiscTextDictionary.LocalizationDict.FirstOrDefault(x => x.Value == itemName);

                        if (string.IsNullOrEmpty(nameId.Value))
                        {
                            Logger.LogError("Could not find name for item '{ItemName}' in misc dictionary", itemName);
                            continue;
                        }

                        Descriptions.TryAdd(nameId.Key, nameId.Value);

                        if (!string.IsNullOrEmpty(prefabName))
                            if (Items.TryGetValue(itemId, out var itemDesc))
                                Logger.LogError("Item '{PrefabName}' cannot be added as item '{PrefabName}' already exists with id '{Id}'", prefabName, itemDesc.PrefabName, itemId);
                            else
                                Items.Add(itemId, new ItemDescription(itemId,
                                    tribe, itemCategoryType, subCategoryType, actionType,
                                    (int)rarity, currency, regularPrice, sellPrice, sellCount,
                                    specialDisplayPrefab, itemName, description, prefabName,
                                    cooldownTime, binding, level, levelRequirement, itemEffects, uniqueInInventory,
                                    storeType, discountedFrom, discountedTo, discountPrice, stockPriority, lootId,
                                    productionStatus, recipeParentItemId, releaseDate, memberOnly, delayUseDuration
                                ));
                    }
                }
            }
        }

        var maxDesc = Descriptions.Max(x => x.Key);

        foreach (var obj in InternalObjective.ObjectivePrefabs)
        {
            maxDesc++;

            if (!Items.ContainsKey(obj.Key))
            {
                Descriptions.Add(maxDesc, obj.Value);

                Items.Add(obj.Key, new ItemDescription(obj.Key, TribeType.Unknown, ItemCategory.Quest,
                    ItemSubCategory.Unknown, ItemActionType.Invalid, 0, CurrencyType.Unknown, 0, 0, 0,
                    string.Empty, obj.Value, obj.Value, obj.Value, 0f, ItemBinding.Unknown, 0, 0, [],
                    false, StoreType.Unknown, DateTime.Now, DateTime.Now, 0, 0, 0, ProductionStatus.Ingame,
                    0, DateTime.Now, false, 0));
            }
            else
                Logger.LogError("Objective with id '{Id}' and prefab '{Name}' already exists in item dictionary!", obj.Key, obj.Value);
        }
    }
}
