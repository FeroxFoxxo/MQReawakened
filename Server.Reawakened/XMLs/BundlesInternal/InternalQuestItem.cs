using Microsoft.Extensions.Logging;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Extensions;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class InternalQuestItem : IBundledXml<InternalQuestItem>
{
    public string BundleName => "InternalQuestItem";
    public BundlePriority Priority => BundlePriority.High;

    public ILogger<InternalQuestItem> Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public Dictionary<int, List<ItemModel>> QuestItemList;

    public InternalQuestItem()
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

        foreach (XmlNode questItemXml in xmlDocument.ChildNodes)
        {
            if (!(questItemXml.Name == "QuestItems")) continue;

            foreach (XmlNode quest in questItemXml.ChildNodes)
            {
                if (!(quest.Name == "Quest")) continue;

                var questId = -1;

                foreach (XmlAttribute questAttribute in quest.Attributes)
                    switch (questAttribute.Name)
                    {
                        case "questId":
                            questId = int.Parse(questAttribute.Value);
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
