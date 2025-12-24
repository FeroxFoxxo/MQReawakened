using A2m.Server;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.XMLs.Data.Portals.Models;
public class PortalConditionModel
{
    public PortalConditionType Type;
    public List<int> RequiredItems;
    public List<int> RequiredQuests;
    public Dictionary<TribeType, int> RequiredLevels;

    public PortalConditionModel(PortalConditionType type, int data)
    {
        Type = type;
        RequiredItems = [];
        RequiredQuests = [];
        RequiredLevels = [];

        switch (type)
        {
            case PortalConditionType.RequiredItem:
                RequiredItems.Add(data);
                break;
            case PortalConditionType.RequiredQuest:
                RequiredQuests.Add(data);
                break;
            case PortalConditionType.RequiredLevel:
                RequiredLevels.Add(TribeType.Crossroads, data);
                break;
            default:
                break;
        }
    }

    public PortalConditionModel(PortalConditionType type, List<int> data)
    {
        Type = type;
        RequiredItems = [];
        RequiredQuests = [];
        RequiredLevels = [];

        switch (type)
        {
            case PortalConditionType.RequiredItem:
                RequiredItems = data;
                break;
            case PortalConditionType.RequiredQuest:
                RequiredQuests = data;
                break;
            default:
                break;
        }
    }

    public PortalConditionModel(PortalConditionType type, Dictionary<TribeType, int> requiredLevels)
    {
        Type = type;
        RequiredItems = [];
        RequiredQuests = [];
        RequiredLevels = requiredLevels;
    }

    public bool CheckConditions(Player player) => Type switch
    {
        PortalConditionType.RequiredItem => CheckRequiredItems(player),
        PortalConditionType.RequiredQuest => CheckRequiredQuests(player),
        PortalConditionType.RequiredLevel => CheckRequiredLevels(player),
        _ => true
    };

    public bool CheckRequiredItems(Player player)
    {
        foreach (var item in RequiredItems)
            if (!player.Character.Inventory.Items.ContainsKey(item))
                return false;
        return true;
    }
    public bool CheckRequiredQuests(Player player)
    {
        foreach (var item in RequiredQuests)
            if (!player.Character.CompletedQuests.Contains(item))
                return false;
        return true;
    }

    public bool CheckRequiredLevels(Player player)
    {
        foreach (var item in RequiredLevels)
            if (!(player.Character.GlobalLevel >= item.Value))
                return false;
        return true;
    }
}
