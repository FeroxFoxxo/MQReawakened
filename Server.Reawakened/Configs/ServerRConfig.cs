using A2m.Server;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;

namespace Server.Reawakened.Configs;

public class ServerRConfig : IRConfig
{
    public int DefaultChatLevel { get; }
    public bool DefaultMemberStatus { get; }

    public string DefaultSignUpExperience { get; }
    public string DefaultTrackingShortId { get; }

    public int RandomKeyLength { get; }
    public int PlayerCap { get; }
    public int ReservedNameCount { get; }
    public int MaxCharacterCount { get; }

    public int DefaultQuest { get; }

    public bool LogSyncState { get; }

    public string LevelSaveDirectory { get; }
    public string LevelDataSaveDirectory { get; }
    public string DataDirectory { get; }

    public string[] DefaultProtocolTypeIgnore { get; }

    public char ChatCommandStart { get; }
    public int ChatCommandPadding { get; }

    public double RoomTickRate { get; }

    public int LogOnLagCount { get; }

    public Dictionary<DebugHandler.DebugVariables, bool> DefaultDebugVariables { get; }

    public ServerRConfig()
    {
        LevelSaveDirectory = InternalDirectory.GetDirectory("XMLs/Levels");
        LevelDataSaveDirectory = InternalDirectory.GetDirectory("XMLs/LevelData");
        DataDirectory = InternalDirectory.GetDirectory("XMLs/FormattedData");

        RoomTickRate = 32;

        RandomKeyLength = 24;
        PlayerCap = 20;
        ReservedNameCount = 4;
        MaxCharacterCount = 3;

        DefaultSignUpExperience = "Console";
        DefaultChatLevel = 3;
        DefaultTrackingShortId = "false";
        DefaultMemberStatus = true;

        DefaultQuest = 802;

        LogSyncState = false;
        DefaultProtocolTypeIgnore = new[] { "ss", "Pp", "ku", "kr" };

        ChatCommandStart = '.';
        ChatCommandPadding = 8;

        LogOnLagCount = 200;

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
