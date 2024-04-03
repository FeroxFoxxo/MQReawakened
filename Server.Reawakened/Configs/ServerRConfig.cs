using A2m.Server;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;

namespace Server.Reawakened.Configs;

public class ServerRConfig : IRConfig
{
    public Dictionary<GameVersion, string> CurrentEvent { get; set; }
    public Dictionary<GameVersion, string> CurrentTimedEvent { get; set; }

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
    public string XMLDirectory { get; }
    public string DataDirectory { get; }

    public string[] DefaultProtocolTypeIgnore { get; }

    public char ChatCommandStart { get; }
    public int ChatCommandPadding { get; }

    public double RoomTickRate { get; }

    public int LogOnLagCount { get; }

    public bool ClearCache { get; }

    public Dictionary<DebugHandler.DebugVariables, bool> DefaultDebugVariables { get; }

    public double KickAfterTime { get; }

    public bool LogAllSyncEvents { get; }
    public int AccessRights { get; }

    public Dictionary<TribeType, int> TutorialTribe2014 { get; }
    public GameVersion GameVersion { get; set; }

    public string[] IgnoredDoors { get; set; }

    public string[] EnemyNameSearch { get; set; }

    public float PlayerWidth { get; set; }
    public float PlayerHeight { get; set; }

    public string FrontPlane { get; set; }
    public string BackPlane { get; set; }

    public Dictionary<string, int> Planes { get; set; }

    public int CashKitAmount { get; }

    public string BreakableComponentName { get; set; }
    public string EnemyComponentName { get; set; }
    public string DailyBoxName { get; set; }

    public int MaximumEntitiesToReturnLog { get; set; }

    public List<string> LoadedAssets { get; set; }

    public long LastClientUpdate { get; set; }
    public long CutOffFor2014 { get; set; }

    public ServerRConfig()
    {
        LevelSaveDirectory = InternalDirectory.GetDirectory("XMLs/Levels");
        LevelDataSaveDirectory = InternalDirectory.GetDirectory("XMLs/LevelData");
        DataDirectory = InternalDirectory.GetDirectory("XMLs/FormattedData");
        XMLDirectory = InternalDirectory.GetDirectory("XMLs/XMLFiles");

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

        CashKitAmount = 100000;

        KickAfterTime = TimeSpan.FromMinutes(5).TotalMilliseconds;

        LogAllSyncEvents = true;
        ClearCache = true;

        AccessRights = (int)UserAccessRight.NoDictionaryChat;

        TutorialTribe2014 = new Dictionary<TribeType, int>
        {
            { TribeType.Shadow, 966 }, // NINJA
            { TribeType.Outlaw, 967 }, // PIRATE
            { TribeType.Wild,   968 }, // ICE
            { TribeType.Bone,   969 }, // OOTU
        };

        GameVersion = GameVersion.vLate2013;

        MaxLevel = 65;

        IgnoredDoors = [
            "PF_GLB_DoorArena01"
        ];

        EnemyComponentName = "EnemyController";
        BreakableComponentName = "BreakableEventController";
        DailyBoxName = "Daily";

        EnemyNameSearch = [
            "PF_Spite_Spiderling",
            "Spite_TeaserSpider_Boss",
            "Spite_Spider_Boss"
        ];

        MaximumEntitiesToReturnLog = 15;

        PlayerHeight = 1f;
        PlayerWidth = 1f;

        LoadedAssets = [];

        CurrentEvent = new Dictionary<GameVersion, string>
        {
            { GameVersion.v2014, "boBegnopS_4102_TVE" },
            { GameVersion.vLate2013, "10TNMT_3102_TVE" },
            { GameVersion.vEarly2013, "TNMT_2102_ORP" },
            { GameVersion.vLate2012, "TNMT_2102_ORP" },
            { GameVersion.vMinigames2012, "regnaRrewoP_2102_ORP" },
            { GameVersion.vPets2012, "regnaRrewoP_2102_ORP" },
            { GameVersion.vEarly2012, "adnaPuFgnuK_2102_ORP" },
            { GameVersion.v2011, string.Empty }
        };

        CurrentTimedEvent = new Dictionary<GameVersion, string>
        {
            { GameVersion.v2014, "tnevEytraPboBegnopS" },
            { GameVersion.vLate2013, string.Empty },
            { GameVersion.vEarly2013, string.Empty },
            { GameVersion.vLate2012, string.Empty },
            { GameVersion.vMinigames2012, string.Empty },
            { GameVersion.vPets2012, string.Empty },
            { GameVersion.vEarly2012, string.Empty },
            { GameVersion.v2011, string.Empty }
        };

        FrontPlane = "Plane1";
        BackPlane = "Plane0";

        Planes = new Dictionary<string, int>()
        {
            { FrontPlane, 0 },
            { BackPlane, 20 }
        };

        LastClientUpdate = 0;
        CutOffFor2014 = 0;
    }
}
