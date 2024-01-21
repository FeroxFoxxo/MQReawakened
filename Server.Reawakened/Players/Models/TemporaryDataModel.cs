using Server.Reawakened.Players.Models.Groups;
using Server.Reawakened.Players.Models.Minigames;
using Server.Reawakened.Players.Models.Trade;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Enums;

namespace Server.Reawakened.Players.Models;

public class TemporaryDataModel
{
    public string GameObjectId { get; set; } = "0";
    public int Direction { get; set; } = 0;

    public bool Invincible { get; set; } = false;
    public bool OnGround { get; set; } = false;

    public Vector3Model Position { get; set; } = new Vector3Model();
    public Vector3Model Velocity { get; set; } = new Vector3Model();

    public ArenaModel ArenaModel { get; set; }
    public TradeModel TradeModel { get; set; }
    public GroupModel Group { get; set; }

    public Dictionary<int, List<string>> CurrentAchievements { get; set; } = [];
}
