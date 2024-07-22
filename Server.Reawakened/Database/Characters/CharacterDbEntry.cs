using A2m.Server;
using Server.Base.Core.Models;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Models.Misc;
using Server.Reawakened.Players.Models.Pets;
using Server.Reawakened.Players.Models.System;

namespace Server.Reawakened.Database.Characters;
public class CharacterDbEntry : PersistantData
{
    // RecipeListModel
    public List<RecipeModel> RecipeList { get; set; }

    // CharacterResistancesModel
    public int InternalDamageResistPointsStandard { get; set; }
    public int InternalDamageResistPointsFire { get; set; }
    public int InternalDamageResistPointsIce { get; set; }
    public int InternalDamageResistPointsPoison { get; set; }
    public int InternalDamageResistPointsLightning { get; set; }
    public int ExternalDamageResistPointsStandard { get; set; }
    public int ExternalDamageResistPointsFire { get; set; }
    public int ExternalDamageResistPointsIce { get; set; }
    public int ExternalDamageResistPointsPoison { get; set; }
    public int ExternalDamageResistPointsLightning { get; set; }
    public int ExternalStatusEffectResistSecondsStun { get; set; }
    public int ExternalStatusEffectResistSecondsSlow { get; set; }
    public int ExternalStatusEffectResistSecondsFreeze { get; set; }

    // LevelData
    public int LevelId { get; set; }
    public string SpawnPointId { get; set; }

    // InventoryModel
    public Dictionary<int, ItemModel> Items { get; set; }

    // HotbarModel
    public Dictionary<int, ItemModel> HotbarButtons { get; set; }

    // StatusEffectsModel
    public Dictionary<ItemEffectType, StatusEffectModel> StatusEffects { get; set; }

    // CharacterCustomDataModel
    public Dictionary<CustomDataProperties, int> Properties { get; set; }
    public Dictionary<CustomDataProperties, ColorModel> Colors { get; set; }

    // Equipment
    public Dictionary<ItemSubCategory, int> EquippedItems { get; set; }
    public List<ItemSubCategory> EquippedBinding { get; set; }

    // CharacterLightModel
    public int PetItemId { get; set; }
    public bool Registered { get; set; }
    public string CharacterName { get; set; }
    public int UserUuid { get; set; }
    public int Gender { get; set; }
    public int MaxLife { get; set; }
    public int CurrentLife { get; set; }
    public int GlobalLevel { get; set; }
    public CharacterLightData.InteractionStatus InteractionStatus { get; set; }
    public TribeType Allegiance { get; set; }
    public bool ForceTribeSelection { get; set; }
    public List<int> DiscoveredStats { get; set; }

    // CharacterDataModel
    public List<QuestStatusModel> QuestLog { get; set; }
    public List<int> CompletedQuests { get; set; }
    public bool PetAutonomous { get; set; }
    public long GuestPassExpiry { get; set; }
    public bool ShouldExpireGuestPass { get; set; }
    public Dictionary<TribeType, bool> TribesDiscovered { get; set; }
    public Dictionary<TribeType, TribeDataModel> TribesProgression { get; set; }
    public List<int> Friends { get; set; }
    public List<int> Blocked { get; set; }
    public List<int> Muted { get; set; }
    public float Cash { get; set; }
    public float NCash { get; set; }
    public int ActiveQuestId { get; set; }
    public int Reputation { get; set; }
    public int ReputationForCurrentLevel { get; set; }
    public int ReputationForNextLevel { get; set; }
    public float SpawnPositionX { get; set; }
    public float SpawnPositionY { get; set; }
    public bool SpawnOnBackPlane { get; set; }
    public int BadgePoints { get; set; }
    public int AbilityPower { get; set; }
    public Dictionary<string, ReportModel> Reports { get; set; }

    // Character Model
    public Dictionary<int, List<int>> CollectedIdols { get; set; }
    public List<EmailHeaderModel> Emails { get; set; }
    public List<EmailMessageModel> EmailMessages { get; set; }
    public List<int> Events { get; set; }
    public Dictionary<int, Dictionary<string, int>> AchievementObjectives { get; set; }
    public Dictionary<string, float> BestMinigameTimes { get; set; }
    public Dictionary<string, DailiesModel> CurrentCollectedDailies { get; set; }
    public Dictionary<string, DailiesModel> CurrentQuestDailies { get; set; }
    public Dictionary<string, PetModel> Pets { get; set; }

    public CharacterDbEntry(string serverData)
    {
        InitializeList();

        var array = serverData.Split('[');

        Gender = int.Parse(array[0]);

        var properties = array[1].Split(':');

        foreach (var prop in properties)
        {
            var values = prop.Split('=');

            if (values.Length != 3)
                continue;

            var key = (CustomDataProperties)int.Parse(values[0]);
            var value = int.Parse(values[1]);
            var color = new ColorModel(values[2]);

            Properties.Add(key, value);
            Colors.Add(key, color);
        }
    }

    public CharacterDbEntry() => InitializeList();

    public void InitializeList()
    {
        // RecipeListModel
        RecipeList = [];

        // InventoryModel
        Items = [];

        // HotbarModel
        HotbarButtons = [];

        // StatusEffectsModel
        StatusEffects = [];

        // CharacterCustomDataModel
        Properties = [];
        Colors = [];

        // Equipment
        EquippedItems = [];
        EquippedBinding = [];

        // CharacterLightModel
        DiscoveredStats = [];

        // CharacterDataModel
        QuestLog = [];
        CompletedQuests = [];
        TribesDiscovered = [];
        TribesProgression = [];
        Friends = [];
        Blocked = [];
        Muted = [];
        Reports = [];

        // Character Model
        CollectedIdols = [];
        Emails = [];
        EmailMessages = [];
        Events = [];
        AchievementObjectives = [];
        BestMinigameTimes = [];
        CurrentCollectedDailies = [];
        CurrentQuestDailies = [];
        Pets = [];
    }
}
