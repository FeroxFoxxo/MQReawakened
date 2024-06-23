using Server.Base.Core.Abstractions;
using Server.Reawakened.Players.Models.Character;

namespace Server.Reawakened.Core.Configs;
public class ItemRConfig : IRConfig
{
    public int HealingStaff { get; }
    public double HealingStaffHealValue { get; }
    public int HealAmount { get; }

    public int DefaultMeleeDamage { get; }
    public int DefaultRangedDamage { get; }
    public int DefaultDropDamage { get; }

    public int[] SingleItemKit { get; }
    public int[] StackedItemKit { get; }
    public int AmountToStack { get; }

    public int HealingStaffID { get; }
    public int MysticCharmID { get; }

    public float ProjectileSpeedX { get; }
    public float ProjectileSpeedY { get; }
    public float ProjectileGravityFactor { get; }
    public float ProjectileXOffset { get; }
    public float ProjectileYOffset { get; }
    public float ProjectileWidth { get; }
    public float ProjectileHeight { get; }

    public float GrenadeSpeedX { get; }
    public float GrenadeSpeedY { get; }
    public float GrenadeGravityFactor { get; }
    public float GrenadeSpawnDelay { get; }
    public float GrenadeLifeTime { get; }

    public float MeleeWidth { get; }
    public float MeleeHeight { get; }
    public float MeleeAerialRange { get; }
    public float MeleeAerialOffset { get; }

    public Dictionary<int, string> TrainingGear { get; }
    public Dictionary<int, List<string>> TrainingGear2011 { get; }
    public float PetPressButtonDelay { get; }
    public float PetHoldChainDelay { get; }
    public float PetPosYOffset { get; }
    public float PetPosOnButtonYOffset { get; }
    public ItemModel EmptySlot { get; }

    public ItemRConfig()
    {
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
        HealAmount = 100000;

        HealingStaff = 396;
        HealingStaffHealValue = 3.527f;
        DefaultMeleeDamage = 22;
        DefaultRangedDamage = 18;
        DefaultDropDamage = 35;

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

        MeleeWidth = 3.5f;
        MeleeHeight = 1f;

        MeleeAerialRange = 5f;
        MeleeAerialOffset = 2.5f;

        TrainingGear = new Dictionary<int, string>
        {
            { 465, "ABIL_GrapplingHook01" }, // lv_shd_teaser01
            { 466, "ABIL_Glider01" }, // lv_out_teaser01
            { 467, "ABIL_MysticCharm01" }, // lv_bon_teaser01
            { 497, "ABIL_SnowBoard01" }, // lv_wld_teaser01
            { 498, "ABIL_SnowBoard02" }, // lv_wld_highway01
        };

        TrainingGear2011 = new Dictionary<int, List<string>>
        {
            { 48, ["ABIL_GrapplingHook01", "Add_SHD_ChimFoo01_S01", "WPN_PRJ_Shuriken01"] }, // lv_shd_highway01
            { 54, ["ABIL_Glider01", "Add_OUT_SeaDragon01_S01", "WPN_MEL_FireSword01"] }, // lv_out_highway01
            { 46, ["ABIL_MysticCharm01", "Add_BON_OotuMystic01_S01", "WPN_PRJ_PoisonousFlower01"] }, // lv_bon_highway01
            { 498, ["ABIL_SnowBoard02", "Add_WLD_IceRaider01_TS01", "WPN_MEL_IceRaiderAxe01"] }, // lv_wld_highway01
        };

        PetPressButtonDelay = 0.5f;
        PetHoldChainDelay = 1f;
        PetPosYOffset = 0.75f;
        PetPosOnButtonYOffset = 0.25f;

        EmptySlot = new ItemModel()
        {
            //Id of 0 crashes game, so we use 340 until a better solution is found.
            ItemId = 340,
            Count = 0,
            BindingCount = 0,
            DelayUseExpiry = DateTime.Now
        };
    }
}
