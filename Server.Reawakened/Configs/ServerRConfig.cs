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

    public int HealingStaff { get; }
    public double HealingStaffHealValue { get; }
    public int DefaultMeleeDamage { get; }
    public int DefaultRangedDamage { get; }
    public int DefaultDropDamage { get; }

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

    public int[] SingleItemKit { get; }
    public int[] StackedItemKit { get; }
    public int AmountToStack { get; }
    public int CashKitAmount { get; }
    public int HealAmount { get; }

    public double KickAfterTime { get; }

    public bool LogAllSyncEvents { get; }
    public int AccessRights { get; }

    public Dictionary<TribeType, int> TutorialTribe2014 { get; }
    public GameVersion GameVersion { get; set; }

    public string[] IgnoredDoors { get; set; }

    public string[] EnemyNameSearch { get; set; }
    public string BreakableComponentName { get; set; }
    public string EnemyComponentName { get; set; }
    public string DailyBoxName { get; set; }
    public string NoEffect { get; }
    public string ToxicCloud { get; }

    public int MaximumEntitiesToReturnLog { get; set; }
    public int HealingStaffID { get; set; }
    public int MysticCharmID { get; set; }
    public float ProjectileSpeedX { get; set; }
    public float ProjectileSpeedY { get; set; }
    public float ProjectileGravityFactor { get; set; }
    public float GrenadeSpeedX { get; set; }
    public float GrenadeSpeedY { get; set; }
    public float GrenadeGravityFactor { get; set; }
    public float GrenadeSpawnDelay { get; set; }
    public float GrenadeLifeTime { get; set; }
    public float ProjectileXOffset { get; set; }
    public float ProjectileYOffset { get; set; }
    public float ProjectileWidth { get; set; }
    public float ProjectileHeight { get; set; }
    public float MeleeXOffset { get; set; }
    public float MeleeYOffset { get; set; }
    public float MeleeWidth { get; set; }
    public float MeleeHeight { get; set; }
    public float MeleeArialXOffset { get; set; }
    public float MeleeArialYOffset { get; set; }
    public float MeleeArialWidth { get; set; }
    public float MeleeArialHeight { get; set; }
    public float PlayerWidth { get; set; }
    public float PlayerHeight { get; set; }
    public string FrontPlane { get; set; }
    public string BackPlane { get; set; }
    public Dictionary<string, int> Planes { get; set; }
    public Dictionary<int, string> TrainingGear { get; set; }
    public Dictionary<int, string> TrainingGear2011 { get; set; }

    public List<string> LoadedAssets { get; set; }

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

        SingleItemKit =
        [
            394,  // glider
            395,  // grappling hook
            9701,  // snowboard
            397,  // wooden slingshot
            423,  // golden slingshot
            453,  // kernel blaster
            1009, // snake staff
            584,  // scrying orb
            2978, // wooden sword
            2883, // oak plank helmet
            2886, // oak plank armor
            2880, // oak plank pants
            1232, // black cat burglar mask
            3152, // super monkey pack
            3053, // boom bomb construction kit
            3023, // ladybug warrior costume pack
            3022, // boom bug pack
            2972, // ace pilot pack
            2973, // crimson dragon pack
            2923, // banana box
            3024, // steel sword
            2878 // cadet training sword
        ];

        StackedItemKit =
        [
            396,  // healing staff
            585,  // invisible bomb
            1568, // red apple
            405,  // healing potion
        ];

        AmountToStack = 99;
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
        HealAmount = 100000;

        IgnoredDoors = [
            "PF_GLB_DoorArena01"
        ];

        HealingStaff = 396;
        HealingStaffHealValue = 3.527f;
        DefaultMeleeDamage = 22;
        DefaultRangedDamage = 18;
        DefaultDropDamage = 35;

        EnemyComponentName = "EnemyController";
        BreakableComponentName = "BreakableEventController";
        DailyBoxName = "Daily";
        NoEffect = "NoEffect";
        ToxicCloud = "ToxicCloud";

        EnemyNameSearch = [
            "PF_Critter_Bird",
            "PF_Critter_Fish",
            "PF_Critter_Spider",
            "PF_Spite_Bathog",
            "PF_Spite_Bomber",
            "PF_Spite_Crawler",
            "PF_Spite_Dragon",
            "PF_Spite_Grenadier",
            "PF_Spite_Orchid",
            "PF_Spite_Pincer",
            "PF_Spite_Stomper",
            "Spite_Wasp_Boss01",
            "PF_Spite_Spiderling",
            "Spite_TeaserSpider_Boss",
            "Spite_Spider_Boss",
            "PF_Spite_Hamster",
            "PF_Spite_Squirrel"
        ];

        MaximumEntitiesToReturnLog = 15;
        HealingStaffID = 396;
        MysticCharmID = 398;

        GrenadeSpeedX = 7f;
        GrenadeSpeedY = 7f;
        GrenadeGravityFactor = 0.25f;
        GrenadeSpawnDelay = 0.5f;
        GrenadeLifeTime = 3f;

        ProjectileSpeedX = 10f;
        ProjectileSpeedY = 0f;

        ProjectileXOffset = 0.25f;
        ProjectileYOffset = 0.8f;
        ProjectileHeight = 0.5f;
        ProjectileWidth = 0.5f;

        MeleeXOffset = 4f;
        MeleeYOffset = 0f;
        MeleeWidth = 4f;
        MeleeHeight = 1f;

        MeleeArialXOffset = 3f;
        MeleeArialYOffset = 2.6f;
        MeleeArialWidth = 6f;
        MeleeArialHeight = 5.2f;

        PlayerHeight = 1f;
        PlayerWidth = 1f;

        TrainingGear = new Dictionary<int, string>
        {
            { 465, "ABIL_GrapplingHook01" }, // lv_shd_teaser01
            { 466, "ABIL_Glider01" }, // lv_out_teaser01
            { 467, "ABIL_MysticCharm01" }, // lv_bon_teaser01
            { 497, "ABIL_SnowBoard01" }, // lv_wld_teaser01
            { 498, "ABIL_SnowBoard02" }, // lv_wld_highway01
        };

        TrainingGear2011 = new Dictionary<int, string>
        {
            { 48, "ABIL_GrapplingHook01" }, // lv_shd_highway01
            { 54, "ABIL_Glider01" }, // lv_out_highway01
            { 46, "ABIL_MysticCharm01" }, // lv_bon_highway01
            { 498, "ABIL_SnowBoard02" }, // lv_wld_highway01
        };

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
    }
}
