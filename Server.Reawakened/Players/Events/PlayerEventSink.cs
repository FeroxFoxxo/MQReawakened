using Server.Base.Core.Abstractions;

namespace Server.Reawakened.Players.Events;

public class PlayerEventSink : IEventSink
{
    public delegate void PlayerRefreshedHandler();

    public void InvokePlayerRefresh() => PlayerRefreshed?.Invoke();

    public event PlayerRefreshedHandler PlayerRefreshed;
}
