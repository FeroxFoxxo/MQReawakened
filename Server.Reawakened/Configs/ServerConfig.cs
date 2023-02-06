using A2m.Server;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;

namespace Server.Reawakened.Configs;

public class ServerConfig : IConfig
{
    public int RandomKeyLength { get; set; }
    public int PlayerCap { get; set; }
    public int ReservedNameCount { get; set; }
    public int MaxCharacterCount { get; set; }

    public int DefaultQuest { get; set; }

    public string DefaultSignUpExperience;
    public int DefaultChatLevel;
    public string DefaultTrackingShortId;
    public bool DefaultMemberStatus;

    public bool LogSyncState { get; set; }

    public string LevelSaveDirectory { get; set; }
    public string LevelDataSaveDirectory { get; set; }

    public string[] DefaultProtocolTypeIgnore { get; set; }

    public Dictionary<DebugHandler.DebugVariables, bool> DefaultDebugVariables { get; set; }

    public ServerConfig()
    {
        LevelSaveDirectory = Path.Combine(InternalDirectory.GetBaseDirectory(), "Level");
        LevelDataSaveDirectory = Path.Combine(InternalDirectory.GetBaseDirectory(), "LevelData");

        RandomKeyLength = 24;
        PlayerCap = 20;
        ReservedNameCount = 4;
        MaxCharacterCount = 3;

        DefaultSignUpExperience = "UNKNOWN";
        DefaultChatLevel = 3;
        DefaultTrackingShortId = "false";
        DefaultMemberStatus = true;

        DefaultQuest = 802;

        LogSyncState = false;
        DefaultProtocolTypeIgnore = new[] { "ss", "Pp", "ku", "kr" };

        DefaultDebugVariables = new Dictionary<DebugHandler.DebugVariables, bool>
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
