using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Services;
using Server.Base.Network.Enums;

namespace Server.Reawakened.Players.Services;

public class CreateNewAccount : IService
{
    private readonly EventSink _eventSink;
    private readonly ServerConsole _serverConsole;

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
            NetworkType.Server,
            AddAccount
        );

    private void AddAccount(string[] command) =>
        _eventSink.InvokeCreateData();
}
