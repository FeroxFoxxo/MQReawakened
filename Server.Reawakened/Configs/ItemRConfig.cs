using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Reawakened.Configs;
public class ItemRConfig
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

    public string NoEffect { get; }
    public string ToxicCloud { get; }

    public int HealingStaffID { get; set; }
    public int MysticCharmID { get; set; }

    public int BreathTimerDuration { get; set; }
    public int UnderwaterDamageInterval { get; set; }

    public float ProjectileSpeedX { get; set; }
    public float ProjectileSpeedY { get; set; }
    public float ProjectileGravityFactor { get; set; }
    public float ProjectileXOffset { get; set; }
    public float ProjectileYOffset { get; set; }
    public float ProjectileWidth { get; set; }
    public float ProjectileHeight { get; set; }

    public float GrenadeSpeedX { get; set; }
    public float GrenadeSpeedY { get; set; }
    public float GrenadeGravityFactor { get; set; }
    public float GrenadeSpawnDelay { get; set; }
    public float GrenadeLifeTime { get; set; }

    public float MeleeXOffset { get; set; }
    public float MeleeYOffset { get; set; }
    public float MeleeWidth { get; set; }
    public float MeleeHeight { get; set; }
    public float MeleeArialXOffset { get; set; }
    public float MeleeArialYOffset { get; set; }
    public float MeleeArialWidth { get; set; }
    public float MeleeArialHeight { get; set; }

    public Dictionary<int, string> TrainingGear { get; set; }
    public Dictionary<int, string> TrainingGear2011 { get; set; }

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
        BreathTimerDuration = 30;
        UnderwaterDamageInterval = 2;

        NoEffect = "NoEffect";
        ToxicCloud = "ToxicCloud";

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
    }
}
