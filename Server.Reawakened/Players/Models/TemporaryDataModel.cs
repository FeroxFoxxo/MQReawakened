using Server.Reawakened.Players.Models.Groups;
using Server.Reawakened.Players.Models.Minigames;
using Server.Reawakened.Players.Models.Trade;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Players.Models;

public class TemporaryDataModel
{
    public int GameObjectId { get; set; }

    public TradeModel TradeModel { get; set; }
    public GroupModel Group { get; set; }

    public bool OnGround { get; set; }
    public int Direction { get; set; }
    public Vector3Model Position { get; set; } = new Vector3Model();
    public Vector3Model Velocity { get; set; } = new Vector3Model();
    public bool Invincible { get; set; } = false;

    public List<int> Events { get; set; } = [];
    public ArenaModel ArenaModel { get; set; } = new ArenaModel();
}
