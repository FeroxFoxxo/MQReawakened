using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Extensions;
using Server.Reawakened.XMLs.Models;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;

internal class InternalQuestItem : IBundledXml
{
    public string BundleName => "InternalQuestItem";
    public BundlePriority Priority => BundlePriority.High;

    public Microsoft.Extensions.Logging.ILogger Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public Dictionary<int, List<ItemModel>> QuestItems;

    public InternalQuestItem()
    {
    }

    public void InitializeVariables() =>
        QuestItems = [];

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

                var itemList = quest.GetXmlItems();

                QuestItems.TryAdd(questId, itemList);
            }
        }
    }

    public void FinalizeBundle()
    {
    }
}
