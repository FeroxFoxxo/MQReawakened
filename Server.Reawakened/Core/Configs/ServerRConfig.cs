using A2m.Server;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Reawakened.Core.Enums;

namespace Server.Reawakened.Core.Configs;

public class ServerRConfig : IRConfig
{
    public Dictionary<GameVersion, string> CurrentEvent { get; }
    public Dictionary<GameVersion, string> CurrentTimedEvent { get; }

    public int DefaultChatLevel { get; }
    public bool DefaultMemberStatus { get; }

    public string DefaultSignUpExperience { get; }
    public string DefaultTrackingShortId { get; }

    public int RandomKeyLength { get; }
    public int PlayerCap { get; }
    public int ReservedNameCount { get; }
    public int MaxCharacterCount { get; }

    public int MaxLevel { get; }
    public int DoubleChestRewardsLevel { get; }
    public int LevelUpNCashReward { get; }

    public int DefaultQuest { get; }

    public bool LogSyncState { get; }

    public string LevelSaveDirectory { get; }
    public string LevelDataSaveDirectory { get; }
    public string XMLDirectory { get; }
    public string DownloadDirectory { get; }
    public string DataDirectory { get; }

    public string[] DefaultProtocolTypeIgnore { get; }

    public double RoomTickRate { get; }

    public int LogOnLagCount { get; }

    public bool ClearCache { get; }

    public Dictionary<DebugHandler.DebugVariables, bool> DefaultDebugVariables { get; }

    public double KickAfterTime { get; }

    public bool LogAllSyncEvents { get; }
    public int AccessRights { get; }

    public Dictionary<TribeType, int> TutorialTribe2014 { get; }

    public string[] IgnoredDoors { get; }

    public float PlayerWidth { get; }
    public float PlayerHeight { get; }

    public string FrontPlane { get; }
    public string BackPlane { get; }

    public Dictionary<string, int> Planes { get; }

    public int CashKitAmount { get; }

    public string DailyBoxName { get; }
    public string BlueChestName { get; }
    public string PurpleChestName { get; }

    public int MaximumEntitiesToReturnLog { get; }

    public GameVersion GameVersion { get; set; }
    public List<string> LoadedAssets { get; set; }
    public long LastClientUpdate { get; set; }
    public long CutOffFor2014 { get; set; }

    public float Gravity { get; }

    public int PetHotbarIndex { get; }

    public string FXWaterSplashName { get; }
    public int BreathTimerDuration { get; }
    public int UnderwaterDamageInterval { get; }
    public int UnderwaterDamageRatio { get; }

    public ServerRConfig()
    {
        LevelSaveDirectory = InternalDirectory.GetDirectory("XMLs/Levels");
        LevelDataSaveDirectory = InternalDirectory.GetDirectory("XMLs/LevelData");
        DataDirectory = InternalDirectory.GetDirectory("XMLs/FormattedData");
        XMLDirectory = InternalDirectory.GetDirectory("XMLs/XMLFiles");
        DownloadDirectory = InternalDirectory.GetDirectory("Downloads");

        RoomTickRate = 32;

        RandomKeyLength = 32;
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
        DoubleChestRewardsLevel = 30;
        LevelUpNCashReward = 175;

        IgnoredDoors = [
            "PF_GLB_DoorArena01"
        ];

        DailyBoxName = "Daily";
        BlueChestName = "Chest02";
        PurpleChestName = "Chest03";

        MaximumEntitiesToReturnLog = 15;

        PlayerHeight = 1f;
        PlayerWidth = 1f;

        LoadedAssets = [];

        CurrentEvent = new Dictionary<GameVersion, string>
        {
            { GameVersion.vLate2014, "boBegnopS_4102_TVE" },
            { GameVersion.vPetMasters2014, "sregnaRrewoP_4102_TVE" },
            { GameVersion.vEarly2014, "10TNMT_3102_TVE" },
            { GameVersion.vLate2013, "10TNMT_3102_TVE" },
            { GameVersion.vEarly2013, "TNMT_2102_ORP" },
            { GameVersion.vLate2012, "TNMT_2102_ORP" },
            { GameVersion.vMinigames2012, "regnaRrewoP_2102_ORP" },
            { GameVersion.vPets2012, "regnaRrewoP_2102_ORP" },
            { GameVersion.vEarly2012, string.Empty },
            { GameVersion.v2011, string.Empty }
        };

        CurrentTimedEvent = new Dictionary<GameVersion, string>
        {
            { GameVersion.vLate2014, "tnevEytraPboBegnopS" },
            { GameVersion.vPetMasters2014, string.Empty },
            { GameVersion.vEarly2014, string.Empty },
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

        Gravity = 15f;

        PetHotbarIndex = 4;

        FXWaterSplashName = "FX_WaterSplash";
        BreathTimerDuration = 31;
        UnderwaterDamageInterval = 2;
        UnderwaterDamageRatio = 10;
    }
}
