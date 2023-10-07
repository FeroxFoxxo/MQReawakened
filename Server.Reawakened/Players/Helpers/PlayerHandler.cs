using Server.Reawakened.Players.Services;
using Server.Reawakened.Rooms.Services;

namespace Server.Reawakened.Players.Helpers;

public class PlayerHandler
{
    public UserInfoHandler UserInfoHandler;

    public List<Player> PlayerList { get; }
    public WorldHandler WorldHandler { get; }
    public object Lock { get; }

    public PlayerHandler(WorldHandler worldHandler)
    {
        WorldHandler = worldHandler;
        PlayerList = new List<Player>();
        Lock = new object();
    }

    public void AddPlayer(Player player)
    {
        lock (Lock)
            PlayerList.Add(player);
    }

    public void RemovePlayer(Player player)
    {
        lock (Lock)
            PlayerList.Remove(player);
    }
}
