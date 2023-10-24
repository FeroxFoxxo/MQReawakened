using Server.Reawakened.Players.Services;
using Server.Reawakened.Rooms.Services;

namespace Server.Reawakened.Players.Helpers;

public class PlayerHandler(WorldHandler worldHandler)
{
    public WorldHandler WorldHandler => worldHandler;
    public UserInfoHandler UserInfoHandler;

    public List<Player> PlayerList { get; } = [];
    public object Lock { get; } = new object();

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
