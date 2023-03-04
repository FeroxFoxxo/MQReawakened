using Server.Reawakened.Players.Services;

namespace Server.Reawakened.Players.Helpers;

public class PlayerHandler
{
    public UserInfoHandler UserInfoHandler;
    public List<Player> PlayerList { get; }
    private readonly object _lock;

    public PlayerHandler()
    {
        PlayerList = new List<Player>();
        _lock = new object();
    }

    public void AddPlayer(Player player)
    {
        lock (_lock)
            PlayerList.Add(player);
    }

    public void RemovePlayer(Player player)
    {
        lock (_lock)
            PlayerList.Remove(player);
    }
}
