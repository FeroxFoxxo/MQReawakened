using A2m.Server;
using Server.Reawakened.Configs;
using Server.Reawakened.Players.Helpers;

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
        ?? new Dictionary<int, int>();

    private PlayerListModel FriendModels =>
        new(FriendList.Select(f => new PlayerDataModel(f.Key, f.Value, _player)).ToList());

    private PlayerListModel BlockModels =>
        new(BlockedList.Select(b => new PlayerDataModel(b.Key, b.Value, _player)).ToList());

    // USER ID + CHARACTER NAME
    public Dictionary<int, int> FriendList { get; set; }
    public Dictionary<int, int> BlockedList { get; set; }
    public Dictionary<int, int> MutedList { get; set; }

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
    public int TraderId;
    public bool AcceptedTrade;
    public bool StoppedTrade;
    public bool TradeDeal;
    public int BananasInTrade;

    private int ChatLevel => _player?.UserInfo.ChatLevel ?? 0;

    public CharacterDataModel() => InitializeDetailedLists();

    public CharacterDataModel(string serverData, int id, ServerRConfig config) : base(serverData)
    {
        Inventory = new InventoryModel();
        Hotbar = new HotbarModel();
        Resistances = new CharacterResistancesModel();
        RecipeList = new RecipeListModel();

        CharacterId = id;
        Customization.CharacterId = id;

        InitializeDetailedLists();

        if (CharacterId > config.MaxCharacterCount || CharacterId < 0)
            throw new InvalidDataException();
    }

    public void SetPlayerData(Player player) =>
        _player = player;

    private void InitializeDetailedLists()
    {
        QuestLog = [];
        CompletedQuests = [];
        TribesDiscovered = [];
        TribesProgression = [];
        DiscoveredStats = [];
        FriendList = [];
        BlockedList = [];
        MutedList = [];
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
        sb.Append(PetItemId);
        sb.Append(PetAutonomous ? 1 : 0);
        sb.Append(GuestPassExpiry);
        sb.Append(ShouldExpireGuestPass ? 1 : 0);
        sb.Append(Registered ? 1 : 0);
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

    public PlayerListModel GetFriends() => FriendModels;
    public PlayerListModel GetBlocked() => BlockModels;
}
