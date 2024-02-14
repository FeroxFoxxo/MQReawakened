using Server.Reawakened.Players.Models.Groups;
using Server.Reawakened.Players.Models.Trade;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Players.Models;

public class TemporaryDataModel
{
    public string GameObjectId { get; set; } = "0";
    public int Direction { get; set; } = 0;

    public bool Invincible { get; set; } = false;
    public bool OnGround { get; set; } = false;
    public bool BananaBoostsElixir { get; set; }
    public bool ReputationBoostsElixir { get; set; }
     
    public Vector3Model Position { get; set; } = new Vector3Model();
    public Vector3Model Velocity { get; set; } = new Vector3Model();

    public TradeModel TradeModel { get; set; }
    public GroupModel Group { get; set; }

    public Dictionary<int, List<string>> CurrentAchievements { get; set; } = [];
    public Dictionary<string, bool> ActiveObjectives { get; set; } = [];

    //Make the player size and such a config option down the line
    public ColliderModel DrawPlayerRect() => new(Position.Z > 10 ? "Plane1" : "Plane0", Position.X - 0.5f, Position.Y - 0.5f, 1, 1);
}
