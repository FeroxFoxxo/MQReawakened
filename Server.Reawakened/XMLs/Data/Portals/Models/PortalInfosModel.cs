using Server.Reawakened.Players;

namespace Server.Reawakened.XMLs.Data.Portals.Models;
public class PortalInfosModel
{
    public List<PortalConditionModel> PortalConditions;
    public bool ShowPremiumPortal;

    public PortalInfosModel(PortalConditionModel condition, bool showPremiumPortal)
    {
        PortalConditions = [condition];
        ShowPremiumPortal = showPremiumPortal;
    }

    public PortalInfosModel(List<PortalConditionModel> conditions, bool showPremiumPortal)
    {
        PortalConditions = conditions;
        ShowPremiumPortal = showPremiumPortal;
    }

    public bool CheckConditions(Player player)
    {
        foreach (var condition in PortalConditions)
            if (!condition.CheckConditions(player))
                return false;
        return true;
    }
}
