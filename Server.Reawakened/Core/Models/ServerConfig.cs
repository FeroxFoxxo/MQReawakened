using A2m.Server;
using Server.Base.Core.Abstractions;

namespace Server.Reawakened.Core.Models;

public class ServerConfig : IConfig
{
    public int RandomKeyLength { get; set; }
    public int PlayerCap { get; set; }
    public int ReservedNameCount { get; set; }
    public int MaxCharacterCount { get; set; }
    public int StartLevel { get; set; }

    public string[] DefaultProtocolTypeIgnore { get; set; }

    public Dictionary<DebugHandler.DebugVariables, bool> DefaultDebugVariables { get; set; }

    public ServerConfig()
    {
        RandomKeyLength = 24;
        PlayerCap = 20;
        ReservedNameCount = 4;
        MaxCharacterCount = 3;
        StartLevel = 47;

        DefaultProtocolTypeIgnore = new[] { "ss", "Pp" };

        DefaultDebugVariables = new Dictionary<DebugHandler.DebugVariables, bool>()
        {
            { DebugHandler.DebugVariables.Sharder_active, false },
            { DebugHandler.DebugVariables.Sharder_1, false },
            { DebugHandler.DebugVariables.Sharder_2, false },
            { DebugHandler.DebugVariables.Ewallet, true },
            { DebugHandler.DebugVariables.Chat, true },
            { DebugHandler.DebugVariables.BugReport, true },
            { DebugHandler.DebugVariables.Crisp, true },
            { DebugHandler.DebugVariables.Trade, true }
        };
    }
}
