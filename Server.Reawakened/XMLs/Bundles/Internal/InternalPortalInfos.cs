using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.XMLs.Abstractions.Enums;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using Server.Reawakened.XMLs.Data.Enemy.Enums;
using Server.Reawakened.XMLs.Data.Portals.Models;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles.Internal;

public class InternalPortalInfos : InternalXml
{
    public override string BundleName => "InternalPortalInfos";
    public override BundlePriority Priority => BundlePriority.High;

    public ILogger<InternalPortalInfos> Logger { get; set; }

    public Dictionary<int, PortalInfosModel> PortalInfos;

    public override void InitializeVariables() => PortalInfos = [];

    public PortalInfosModel GetPortalInfos(int levelId, int goId) => PortalInfos.TryGetValue(goId, out var info) ? info : null;

    public override void ReadDescription(XmlDocument xmlDocument)
    {
        foreach (XmlNode portalInfosXml in xmlDocument.ChildNodes)
        {
            if (portalInfosXml.Name != "PortalInfos") continue;

            foreach (XmlNode portal in portalInfosXml.ChildNodes)
            {
                if (portal.Name != "Portal") continue;

                var goId = 0;
                var levelId = 0;
                var isPremium = true;
                var portalConditionsList = new List<PortalConditionModel>();

                foreach (XmlAttribute portalObjInfo in portal.Attributes)
                    switch (portalObjInfo.Name)
                    {
                        case "id":
                            goId = int.Parse(portalObjInfo.Value);
                            break;
                        case "levelId":
                            levelId = int.Parse(portalObjInfo.Value);
                            break;
                        case "isPremium":
                            isPremium = bool.Parse(portalObjInfo.Value);
                            break;
                    }

                foreach (XmlNode portalConditions in portal.ChildNodes)
                {
                    var conditionType = PortalConditionType.Unknown;
                    var reqItems = new List<int>();
                    var reqQuests = new List<int>();
                    var reqLevels = new Dictionary<TribeType, int>();

                    switch (portalConditions.Name)
                    {
                        case "RequiredItem":
                            foreach (XmlAttribute itemInfo in portalConditions.Attributes)
                                switch (itemInfo.Name)
                                {
                                    case "id":
                                        reqItems.Add(int.Parse(itemInfo.Value));
                                        break;
                                }
                            conditionType = PortalConditionType.RequiredItem;
                            break;
                        case "RequiredQuest":
                            foreach (XmlAttribute questInfo in portalConditions.Attributes)
                                switch (questInfo.Name)
                                {
                                    case "id":
                                        reqQuests.Add(int.Parse(questInfo.Value));
                                        break;
                                }
                            conditionType = PortalConditionType.RequiredQuest;
                            break;
                        case "RequiredLevel":
                            var minLevel = 0;
                            var tribe = TribeType.Crossroads;
                            foreach (XmlAttribute levelsInfo in portalConditions.Attributes)
                            {
                                switch (levelsInfo.Name)
                                {
                                    case "level":
                                        minLevel = int.Parse(levelsInfo.Value);
                                        break;
                                    case "tribe":
                                        switch (levelsInfo.Value)
                                        {
                                            case "Crossroads":
                                            case "Global":
                                                break;
                                            case "Outlaw":
                                                tribe = TribeType.Outlaw;
                                                break;
                                            case "Shadow":
                                                tribe = TribeType.Shadow;
                                                break;
                                            case "Bone":
                                                tribe = TribeType.Bone;
                                                break;
                                            case "Wild":
                                                tribe = TribeType.Wild;
                                                break;
                                            case "Grease":
                                                tribe = TribeType.Grease;
                                                break;
                                            default:
                                                Logger.LogError("Unknown tribe XP type: '{Type}' for portal '{Portal}', defaulting to global...", levelsInfo.Value, goId);
                                                break;
                                        }
                                        break;
                                }
                                reqLevels.TryAdd(tribe, minLevel);
                            }
                            conditionType = PortalConditionType.RequiredLevel;
                            break;
                        default:
                            Logger.LogError("Unknown portal condition type: '{Condition}', skipping...", portalConditions.Name);
                            break;
                    }

                    switch (conditionType)
                    {
                        case PortalConditionType.RequiredItem:
                            portalConditionsList.Add(new PortalConditionModel(conditionType, reqItems));
                            break;
                        case PortalConditionType.RequiredQuest:
                            portalConditionsList.Add(new PortalConditionModel(conditionType, reqQuests));
                            break;
                        case PortalConditionType.RequiredLevel:
                            portalConditionsList.Add(new PortalConditionModel(conditionType, reqLevels));
                            break;
                    }
                }
                var infosModel = new PortalInfosModel(portalConditionsList, isPremium);

                PortalInfos.TryAdd(goId, infosModel);
            }
        }
    }
}
