using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Services;

namespace Server.Reawakened.Players.Services;

public class CreateNewAccount : IService
{
    private readonly ServerConsole _serverConsole;
    private readonly EventSink _eventSink;

    public CreateNewAccount(ServerConsole serverConsole,
        EventSink eventSink)
    {
        _serverConsole = serverConsole;
        _eventSink = eventSink;
    }

    public void Initialize() =>
        _serverConsole.AddCommand(
            "createAccount",
            "Adds a new account to the server.",
            AddAccount
        );

    private void AddAccount(string[] command) =>
        _eventSink.InvokeCreateData();
}
