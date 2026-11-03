using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.XMLs.Abstractions.Enums;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Data.Portals.Models;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles.Internal;

public class InternalPortalInfos : InternalXml
{
    public override string BundleName => "InternalPortalInfos";
    public override BundlePriority Priority => BundlePriority.High;

    public ILogger<InternalPortalInfos> Logger { get; set; }

    public Dictionary<int, PortalInfosModel> PortalInfos;
    
    public ItemCatalog ItemCatalog { get; set; }

    public override void InitializeVariables() => PortalInfos = [];

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
                    var reqItems = new List<int>();
                    var reqQuests = new List<int>();
                    var reqLevel = 0;
                    var portalCondition = new PortalConditionModel([], [], 0, null, null);

                    foreach (XmlAttribute portalObjInfo in portal.Attributes)
                        switch (portalObjInfo.Name)
                        {
                            case "id":
                                goId = int.Parse(portalObjInfo.Value);
                                break;
                            case "toLevelId":
                                levelId = int.Parse(portalObjInfo.Value);
                                break;
                            case "isPremium":
                                isPremium = bool.Parse(portalObjInfo.Value);
                                break;
                        }

                    foreach (XmlNode portalConditions in portal.ChildNodes)
                    {
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
                                break;
                            case "RequiredQuest":
                                foreach (XmlAttribute questInfo in portalConditions.Attributes)
                                    switch (questInfo.Name)
                                    {
                                        case "id":
                                            reqQuests.Add(int.Parse(questInfo.Value));
                                            break;
                                    }
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
                                    }
                                    reqLevel = minLevel;
                                }
                                break;
                            default:
                                Logger.LogError("Unknown portal condition type: '{Condition}', skipping...", portalConditions.Name);
                                break;
                        }
                    }
                    
                    portalCondition = new PortalConditionModel(reqItems, reqQuests, reqLevel, ItemCatalog, ItemCatalog.QuestCatalog);
                    
                    var infosModel = new PortalInfosModel(portalCondition, isPremium);

                    PortalInfos.TryAdd(levelId, infosModel);
                }
        }
    }

    public PortalInfosModel GetPortalInfos(int levelId, int goId)
    {
        if (PortalInfos.TryGetValue(levelId, out var info))
            return info;

        return null;
    }
}
