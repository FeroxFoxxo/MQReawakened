using WorldGraphDefines;

namespace Server.Reawakened.Rooms;

public class Level
{
    public readonly Dictionary<int, Room> Rooms;
    public readonly LevelInfo LevelInfo;

    public Level(LevelInfo levelInfo)
    {
        LevelInfo = levelInfo;
        Rooms = new Dictionary<int, Room>();
    }
}
