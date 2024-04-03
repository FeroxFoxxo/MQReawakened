using A2m.Server;
using Server.Reawakened.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Players.Models.Character;

public class CharacterDataModel : CharacterLightModel
{
    private Player _player;

    public InventoryModel Inventory { get; set; }
    public List<QuestStatusModel> QuestLog { get; set; }
    public List<int> CompletedQuests { get; set; }
    public HotbarModel Hotbar { get; set; }
    public bool PetAutonomous { get; set; }
    public long GuestPassExpiry { get; set; }
    public bool ShouldExpireGuestPass { get; set; }
    public CharacterResistancesModel Resistances { get; set; }
    public RecipeListModel RecipeList { get; set; }
    public Dictionary<TribeType, bool> TribesDiscovered { get; set; }
    public Dictionary<TribeType, TribeDataModel> TribesProgression { get; set; }

    private Dictionary<int, int> IdolCount =>
        _player?.Character.CollectedIdols
            .ToDictionary(x => x.Key, x => x.Value.Count)
        ?? [];

    private PlayerListModel FriendModels =>
        new(Friends.Select(f => new CharacterRelationshipModel(f, _player)).ToList());

    private PlayerListModel BlockModels =>
        new(Blocked.Select(b => new CharacterRelationshipModel(b, _player)).ToList());

    public List<int> Friends { get; set; }
    public List<int> Blocked { get; set; }
    public List<int> Muted { get; set; }

    public int Cash { get; set; }
    public int NCash { get; set; }
    public int ActiveQuestId { get; set; }
    public int Reputation { get; set; }
    public int ReputationForCurrentLevel { get; set; }
    public int ReputationForNextLevel { get; set; }
    public float SpawnPositionX { get; set; }
    public float SpawnPositionY { get; set; }
    public bool SpawnOnBackPlane { get; set; }
    public int BadgePoints { get; set; }
    public int AbilityPower { get; set; }

    private int ChatLevel => _player?.UserInfo.ChatLevel ?? 0;

    private GameVersion _version = GameVersion.Unknown;

    public CharacterDataModel() => InitializeDetailedLists();

    public CharacterDataModel(string serverData) : base(serverData)
    {
        Inventory = new InventoryModel();
        Hotbar = new HotbarModel();
        Resistances = new CharacterResistancesModel();
        RecipeList = new RecipeListModel();

        InitializeDetailedLists();
    }

    public void SetCharacterId(int id)
    {
        CharacterId = id;
        Customization.CharacterId = id;
    }

    public void SetDynamicData(Player player, ServerRConfig config)
    {
        _version = config.GameVersion;
        _player = player;
        _player.NetState.Identifier = CharacterName;
        SetVersion(config.GameVersion);
    }

    private void InitializeDetailedLists()
    {
        QuestLog = [];
        CompletedQuests = [];
        TribesDiscovered = [];
        TribesProgression = [];
        DiscoveredStats = [];
        Friends = [];
        Blocked = [];
        Muted = [];
        InitializeLiteLists();
    }

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('[');

        sb.Append(GetCharacterDataInternal());
        sb.Append(Customization);
        sb.Append(Inventory);
        sb.Append(GetQuestStatusList());
        sb.Append(GetCompletedQuestList());
        sb.Append(Hotbar);
        sb.Append(FriendModels);
        sb.Append(BlockModels);
        sb.Append(Equipment);

        if (_version >= GameVersion.vPets2012)
            sb.Append(PetItemId);

        if (_version >= GameVersion.vLate2012)
            sb.Append(PetAutonomous ? 1 : 0);

        if (_version >= GameVersion.vMinigames2012)
        {
            sb.Append(GuestPassExpiry);
            sb.Append(ShouldExpireGuestPass ? 1 : 0);
            sb.Append(Registered ? 1 : 0);
        }
        
        sb.Append(Resistances);
        sb.Append(RecipeList);
        sb.Append(BuildTribesDiscoveredString());
        sb.Append(BuildIdolCountString());
        sb.Append(BuildStatsDiscoveredString());

        return sb.ToString();
    }

    private string GetCompletedQuestList()
    {
        var sb = new SeparatedStringBuilder('&');

        foreach (var completedQuest in CompletedQuests)
            sb.Append(completedQuest);

        return sb.ToString();
    }

    private string GetQuestStatusList()
    {
        var sb = new SeparatedStringBuilder('&');

        foreach (var qs in QuestLog)
            sb.Append(qs);

        return sb.ToString();
    }

    private string GetCharacterDataInternal()
    {
        var sb = new SeparatedStringBuilder('<');

        sb.Append(CharacterId);
        sb.Append(CharacterName);
        sb.Append(Gender);
        sb.Append(Cash);
        sb.Append(NCash);
        sb.Append(ActiveQuestId);
        sb.Append(MaxLife);
        sb.Append(string.Empty);
        sb.Append(CurrentLife);
        sb.Append(GlobalLevel);
        sb.Append((int)InteractionStatus);
        sb.Append(Reputation);
        sb.Append(ReputationForCurrentLevel);
        sb.Append(ReputationForNextLevel);
        sb.Append(SpawnPositionX);
        sb.Append(SpawnPositionY);
        sb.Append(SpawnOnBackPlane ? 1 : 0);
        sb.Append(BadgePoints);
        sb.Append((int)Allegiance);
        sb.Append(AbilityPower);
        sb.Append(ChatLevel);

        foreach (var tribeData in BuildTribeDataString())
            sb.Append(tribeData);

        return sb.ToString();
    }

    private string BuildTribesDiscoveredString()
    {
        var sb = new SeparatedStringBuilder('<');

        foreach (var kvp in TribesDiscovered)
        {
            sb.Append((int)kvp.Key);
            sb.Append(kvp.Value ? 1 : 0);
        }

        return sb.ToString();
    }

    private string BuildIdolCountString()
    {
        var sb = new SeparatedStringBuilder('<');

        foreach (var kvp in IdolCount)
        {
            sb.Append(kvp.Key);
            sb.Append(kvp.Value);
        }

        return sb.ToString();
    }

    private string BuildStatsDiscoveredString()
    {
        var sb = new SeparatedStringBuilder('<');

        foreach (var stat in DiscoveredStats)
            sb.Append(stat);

        return sb.ToString();
    }

    private string[] BuildTribeDataString() =>
        TribesProgression.Values.Select(tribeType => tribeType.ToString()).ToArray();

    public string BuildPortalData()
    {
        var sb = new SeparatedStringBuilder('|');

        sb.Append(SpawnPositionX);
        sb.Append(SpawnPositionY);
        sb.Append(SpawnOnBackPlane ? 1 : 0);

        return sb.ToString();
    }

    public int CalculateDefense(ItemEffectType effect, ItemCatalog itemCatalog)
    {
        var statManager = new CharacterStatsManager(CharacterName);
        var defense = GameFlow.StatisticData.GetValue(ItemEffectType.Defence, WorldStatisticsGroup.Player, _player.Character.Data.GlobalLevel);
        var itemList = new List<ItemDescription>();

        var defenseType = ItemEffectType.Defence;
        switch (effect)
        {
            case ItemEffectType.FireDamage:
                defenseType = ItemEffectType.ResistFire;
                break;
            case ItemEffectType.EarthDamage:
                defenseType = ItemEffectType.ResistEarth;
                break;
            case ItemEffectType.AirDamage:
                defenseType = ItemEffectType.ResistAir;
                break;
            case ItemEffectType.IceDamage:
                defenseType = ItemEffectType.ResistIce;
                break;
            case ItemEffectType.LightningDamage:
                defenseType = ItemEffectType.ResistLightning;
                break;
            case ItemEffectType.PoisonDamage:
                defenseType = ItemEffectType.ResistEarth;
                break;
        }

        foreach (var item in Equipment.EquippedItems)
            itemList.Add(itemCatalog.GetItemFromId(item.Value));

        defense += statManager.ComputeEquimentBoost(defenseType, itemList);

        return defense;
    }

    public int CalculateDamage(ItemDescription usedItem, ItemCatalog itemCatalog)
    {
        var statManager = new CharacterStatsManager(CharacterName);
        var damage = GameFlow.StatisticData.GetValue(ItemEffectType.AbilityPower, WorldStatisticsGroup.Player, _player.Character.Data.GlobalLevel);
        var itemList = new List<ItemDescription> { usedItem };

        var effect = ItemEffectType.BluntDamage;
        switch (usedItem.Elemental)
        {
            case Elemental.Fire:
                effect = ItemEffectType.FireDamage;
                break;
            case Elemental.Earth:
                effect = ItemEffectType.EarthDamage;
                break;
            case Elemental.Air:
                effect = ItemEffectType.AirDamage;
                break;
            case Elemental.Ice:
                effect = ItemEffectType.IceDamage;
                break;
            case Elemental.Lightning:
                effect = ItemEffectType.LightningDamage;
                break;
            case Elemental.Poison:
                effect = ItemEffectType.EarthDamage;
                break;
        }

        foreach (var item in Equipment.EquippedItems)
            itemList.Add(itemCatalog.GetItemFromId(item.Value));

        damage += + statManager.ComputeEquimentBoost(effect, itemList);

        return damage;
    }

    public PlayerListModel GetFriends() => FriendModels;
    public PlayerListModel GetBlocked() => BlockModels;
}
