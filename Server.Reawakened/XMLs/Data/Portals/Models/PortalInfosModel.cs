using Server.Reawakened.Players;

namespace Server.Reawakened.XMLs.Data.Portals.Models;
public class PortalInfosModel
{
    public PortalConditionModel PortalCondition;
    public bool ShowPremiumPortal;

    public PortalInfosModel(PortalConditionModel condition, bool showPremiumPortal)
    {
        PortalCondition = condition;
        ShowPremiumPortal = showPremiumPortal;
    }

    public bool CheckConditions(Player player) => PortalCondition.CheckConditions(player);
}
