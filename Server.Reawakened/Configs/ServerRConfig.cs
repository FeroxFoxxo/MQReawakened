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

    public int MaxLevel { get; }

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

    public bool ClearCache { get; }

    public Dictionary<DebugHandler.DebugVariables, bool> DefaultDebugVariables { get; }

    public int[] SingleItemKit { get; }
    public int[] StackedItemKit { get; }
    public int AmountToStack { get; }
    public int CashKitAmount { get; }

    public double KickAfterTime { get; }

    public bool LogAllSyncEvents { get; }
    public int AccessRights { get; }

    public Dictionary<TribeType, int> TutorialTribe2014 { get; }

    public bool Is2014Client { get; set; }

    public ServerRConfig()
    {
        LevelSaveDirectory = InternalDirectory.GetDirectory("XMLs/Levels");
        LevelDataSaveDirectory = InternalDirectory.GetDirectory("XMLs/LevelData");
        DataDirectory = InternalDirectory.GetDirectory("XMLs/FormattedData");

        RoomTickRate = 32;

        RandomKeyLength = 24;
        PlayerCap = 200;
        ReservedNameCount = 4;
        MaxCharacterCount = 3;

        DefaultSignUpExperience = "Console";
        DefaultChatLevel = 3;
        DefaultTrackingShortId = "false";
        DefaultMemberStatus = true;

        DefaultQuest = 802;

        LogSyncState = false;
        DefaultProtocolTypeIgnore = ["ss", "Pp", "ku", "kr"];

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

        SingleItemKit =
        [
            394, // glider
            395, // grappling hook
            397, // wooden slingshot
            453, // kernel blaster
            2978, // wooden sword
            2883, // oak helmet
            2886, // oak armor
            2880, // oak pants
            1232, // burglar mask
            3152, // super monkey
            3053, // boom bomb
            3023, // warrior costume
            3022, // boom bug
            2972, // ace pilot
            2973, // crimson dragon
            2923, // banana box
        ];

        StackedItemKit =
        [
            396, // healing staff
            585, // invisible bomb
            1568, // red apple
            1704, // healing potion
        ];

        AmountToStack = 98;
        CashKitAmount = 100000;

        KickAfterTime = TimeSpan.FromMinutes(5).TotalMilliseconds;

        LogAllSyncEvents = true;
        ClearCache = true;

        AccessRights = (int) UserAccessRight.NoDictionaryChat;

        TutorialTribe2014 = new Dictionary<TribeType, int>
        {
            { TribeType.Shadow, 966 }, // NINJA
            { TribeType.Outlaw, 967 }, // PIRATE
            { TribeType.Wild,   968 }, // ICE
            { TribeType.Bone,   969 }, // OOTU
        };

        Is2014Client = false;

        MaxLevel = 65;
    }
}
