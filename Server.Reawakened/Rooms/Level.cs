using WorldGraphDefines;

namespace Server.Reawakened.Rooms;

public class Level(LevelInfo levelInfo)
{
    public readonly Dictionary<int, Room> Rooms = new();
    public readonly LevelInfo LevelInfo = levelInfo;
}
