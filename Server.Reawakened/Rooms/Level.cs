using WorldGraphDefines;

namespace Server.Reawakened.Rooms;

public class Level(LevelInfo levelInfo)
{
    public readonly object Lock = new();
    public readonly Dictionary<int, Room> Rooms = [];
    public readonly LevelInfo LevelInfo = levelInfo;
}
