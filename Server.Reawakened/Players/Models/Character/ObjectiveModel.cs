using A2m.Server;

namespace Server.Reawakened.Players.Models.Character;

public class ObjectiveModel
{
    public bool Completed { get; set; }
    public int CountLeft { get; set; }
    public int Total { get; set; }

    public ObjectiveEnum ObjectiveType { get; set; }
    public int Order { get; set; }

    public int GameObjectId { get; set; }
    public int GameObjectLevelId { get; set; }

    public int LevelId { get; set; }
    public int ItemId { get; set; }
}
