using Server.Reawakened.Players.Services;

namespace Server.Reawakened.Players.Helpers;

public class PlayerHandler
{
    public UserInfoHandler UserInfoHandler;
    public List<Player> PlayerList { get; }
    public readonly object Lock;

    public PlayerHandler()
    {
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
