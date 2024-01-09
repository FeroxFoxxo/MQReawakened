using Server.Reawakened.Players.Models.Character;
using System.Xml;

namespace Server.Reawakened.XMLs.Extensions;

public static class GetItems
{
    public static List<ItemModel> GetXmlItems(this XmlNode node)
    {
        var itemList = new List<ItemModel>();

        foreach (XmlNode items in node.ChildNodes)
        {
            if (items.Name != "Item")
                continue;

            var itemId = -1;
            var count = -1;
            var bindingCount = -1;
            var delayUseExpiry = DateTime.Now;
            var weight = 1;

            foreach (XmlAttribute itemtAttributes in items.Attributes)
            {
                switch (itemtAttributes.Name)
                {
                    case "itemId":
                        itemId = int.Parse(itemtAttributes.Value);
                        break;
                    case "count":
                        count = int.Parse(itemtAttributes.Value);
                        break;
                    case "bindingCount":
                        bindingCount = int.Parse(itemtAttributes.Value);
                        break;
                    case "delayUseExpiry":
                        delayUseExpiry = DateTime.Parse(itemtAttributes.Value);
                        break;
                    case "weight":
                        weight = int.Parse(itemtAttributes.Value);
                        break;
                }
            }
            itemList.Add(new ItemModel()
            {
                ItemId = itemId,
                Count = count,
                BindingCount = bindingCount,
                DelayUseExpiry = delayUseExpiry,
                Weight = weight
            });
        }

        return itemList;
    }
}
