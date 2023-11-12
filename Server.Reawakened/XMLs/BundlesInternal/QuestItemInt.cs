using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Extensions;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class QuestItemInt : IBundledXml
{
    public string BundleName => "QuestItemInt";
    public BundlePriority Priority => BundlePriority.High;

    public Microsoft.Extensions.Logging.ILogger Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public Dictionary<int, List<ItemModel>> QuestItemList;

    public QuestItemInt()
    {
    }

    public void InitializeVariables() =>
        QuestItemList = [];

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
                    switch (itemAttributes.Name)
                    {
                        case "questId":
                            questId = int.Parse(itemAttributes.Value);
                            break;
                    }

                var itemList = quest.GetXmlItems();

                QuestItemList.TryAdd(questId, itemList);
            }
        }
    }

    public void FinalizeBundle()
    {
    }
}
