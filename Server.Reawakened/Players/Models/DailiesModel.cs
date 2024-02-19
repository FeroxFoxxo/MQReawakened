using A2m.Server;
using Server.Reawakened.Players.Helpers;
using static A2m.Server.QuestStatus;

namespace Server.Reawakened.Players.Models.Character;

public class DailiesModel
{
    public string GameObjectId { get; set; }
    public int LevelId { get; set; }
    public DateTime TimeOfHarvest { get; set; } 
}
