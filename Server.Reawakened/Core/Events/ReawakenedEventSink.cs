using Server.Base.Core.Abstractions;

namespace Server.Reawakened.Core.Events;
public class ReawakenedEventSink : IEventSink
{
    public delegate void ServerAddressFoundEventHandler();

    public void InvokeServerAddressFound() => ServerAddressFound?.Invoke();

    public event ServerAddressFoundEventHandler ServerAddressFound;
}
