using Microsoft.Extensions.Logging;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Extensions;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class InternalEventReward : IBundledXml<InternalEventReward>
{
    public string BundleName => "InternalEventReward";
    public BundlePriority Priority => BundlePriority.Medium;

    public ILogger<InternalEventReward> Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public Dictionary<int, Dictionary<RewardType, int>> EventRewards;

    public void InitializeVariables() => EventRewards = [];

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        foreach (XmlNode eventRewardXml in xmlDocument.ChildNodes)
        {
            if (eventRewardXml.Name != "EventRewards") continue;

            foreach (XmlNode tEvent in eventRewardXml.ChildNodes)
            {
                if (tEvent.Name != "Event") continue;

                var eventId = -1;
                var rewards = new Dictionary<RewardType, int>();

                foreach (XmlAttribute eventAttribute in tEvent.Attributes!)
                    switch (eventAttribute.Name)
                    {
                        case "eventId":
                            eventId = int.Parse(eventAttribute.Value);
                            continue;
                    }

                foreach (XmlNode eventReward in tEvent.ChildNodes)
                {
                    if (!(eventReward.Name == "Reward")) continue;

                    var rewardType = RewardType.NickCash;
                    var rewardAmount = -1;

                    foreach (XmlAttribute rewardAttribute in eventReward.Attributes!)
                        switch (rewardAttribute.Name)
                        {
                            case "rewardType":
                                rewardType = rewardType.GetEnumValue(rewardAttribute.Value, Logger);
                                continue;
                            case "rewardAmount":
                                rewardAmount = int.Parse(rewardAttribute.Value);
                                continue;
                        }

                    rewards.Add(rewardType, rewardAmount);
                }

                EventRewards.Add(eventId, rewards);
            }
        }
    }

    public void FinalizeBundle()
    {
    }

    public void CheckEventReward(int eventId, Player player)
    {
        if (!EventRewards.TryGetValue(eventId, out var eventRewards))
            return;

        foreach (var eventReward in eventRewards)
            switch (eventReward.Key)
            {
                case RewardType.NickCash:
                    player.AddNCash(eventReward.Value);
                    break;
                case RewardType.Bananas:
                    player.AddBananas(eventReward.Value);
                    break;
                case RewardType.Unknown:
                    Logger.LogError("Unknown reward type {Type} for event id {Id}", eventReward.Key, eventId);
                    break;
            }
    }
}
