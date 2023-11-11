using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;

internal class InternalQuestItem : IBundledXml
{
    public string BundleName => "InternalQuestItem";
    public BundlePriority Priority => BundlePriority.High;

    public Microsoft.Extensions.Logging.ILogger Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public Dictionary<int, List<ItemModel>> Quests;

    public InternalQuestItem()
    {
    }

    public void InitializeVariables() =>
        Quests = [];

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        foreach (XmlNode quests in xmlDocument.ChildNodes)
        {
            if (!(quests.Name == "QuestItems")) continue;

            foreach (XmlNode quest in quests.ChildNodes)
            {
                if (!(quest.Name == "Quest")) continue;

                var questId = -1;

                foreach (XmlAttribute itemAttributes in quest.Attributes)
                {
                    switch (itemAttributes.Name)
                    {
                        case "questId":
                            questId = int.Parse(itemAttributes.Value);
                            break;
                    }
                }

                var itemList = new List<ItemModel>();

                foreach (XmlNode items in quest.ChildNodes)
                {
                    if (items.Name != "Item") continue;

                    var itemId = -1;
                    var count = -1;

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
                        }
                    }
                    itemList.Add(new ItemModel(itemId, count));
                }

                if (!Quests.ContainsKey(questId))
                    Quests.Add(questId, itemList);
            }
        }
    }

    public void FinalizeBundle()
    {
    }
}
