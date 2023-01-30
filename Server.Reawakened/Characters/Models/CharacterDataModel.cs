using A2m.Server;
using Server.Reawakened.Characters.Helpers;
using Server.Reawakened.Core.Models;
using System.Globalization;

namespace Server.Reawakened.Characters.Models;

public class CharacterDataModel : CharacterLightModel
{
    public const char FieldJoiner = '&';
    public const char PortalJoiner = '|';

    public InventoryModel Inventory { get; set; }
    public List<QuestStatusModel> QuestLog { get; set; }
    public List<int> CompletedQuests { get; set; }
    public HotbarModel Hotbar { get; set; }
    public FriendListModel FriendList { get; set; }
    public BlockListModel BlockList { get; set; }
    public bool PetAutonomous { get; set; }
    public long GuestPassExpiry { get; set; }
    public bool ShouldExpireGuestPass { get; set; }
    public CharacterResistancesModel Resistances { get; set; }
    public RecipeListModel RecipeList { get; set; }
    public Dictionary<TribeType, bool> TribesDiscovered { get; set; }
    public Dictionary<int, int> IdolCount { get; set; }
    public bool SpawnOnBackPlane { get; set; }
    public float SpawnPositionX { get; set; }
    public float SpawnPositionY { get; set; }
    public int BadgePoints { get; set; }
    public int Cash { get; set; }
    public int NCash { get; set; }
    public int ActiveQuestId { get; set; }
    public int Reputation { get; set; }
    public int ReputationForCurrentLevel { get; set; }
    public int ReputationForNextLevel { get; set; }
    public int AbilityPower { get; set; }
    public Dictionary<TribeType, TribeDataModel> TribesProgression { get; set; }
    public int ChatLevel { get; set; }

    public CharacterDataModel() {}

    public CharacterDataModel(string serverData, ServerConfig config) : base(serverData, config)
    {
        QuestLog = new List<QuestStatusModel>();
        CompletedQuests = new List<int>();
        TribesDiscovered = new Dictionary<TribeType, bool>();
        IdolCount = new Dictionary<int, int>();
        TribesProgression = new Dictionary<TribeType, TribeDataModel>();
        DiscoveredStats = new HashSet<int>();

        Inventory = new InventoryModel();
        Hotbar = new HotbarModel();
        FriendList = new FriendListModel();
        BlockList = new BlockListModel();
        Resistances = new CharacterResistancesModel();
        RecipeList = new RecipeListModel();
    }

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder(CharacterDataEndDelimiter);

        sb.Append(GetCharacterDataInternal());
        sb.Append(Customization);
        sb.Append(Inventory);
        sb.Append(GetQuestStatusList());
        sb.Append(GetCompletedQuestList());
        sb.Append(Hotbar);
        sb.Append(FriendList);
        sb.Append(BlockList);
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
        var sb = new SeparatedStringBuilder(FieldJoiner);

        foreach (var completedQuest in CompletedQuests)
            sb.Append(completedQuest);

        return sb.ToString();
    }

    private string GetQuestStatusList()
    {
        var sb = new SeparatedStringBuilder(FieldJoiner);

        foreach (var qs in QuestLog)
            sb.Append(qs);

        return sb.ToString();
    }

    private string GetCharacterDataInternal()
    {
        var sb = new SeparatedStringBuilder(FieldSeparator);
        
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
        sb.Append(InteractionStatus);
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
        var sb = new SeparatedStringBuilder(FieldSeparator);

        foreach (var kvp in TribesDiscovered)
        {
            sb.Append((int)kvp.Key);
            sb.Append(kvp.Value ? 1 : 0);
        }

        return sb.ToString();
    }

    private string BuildIdolCountString()
    {
        var sb = new SeparatedStringBuilder(FieldSeparator);

        foreach (var kvp in IdolCount)
        {
            sb.Append(kvp.Key);
            sb.Append(kvp.Value);
        }

        return sb.ToString();
    }

    private string BuildStatsDiscoveredString()
    {
        var sb = new SeparatedStringBuilder(FieldSeparator);

        foreach (var stat in DiscoveredStats)
            sb.Append(stat.ToString(CultureInfo.InvariantCulture));

        return sb.ToString();
    }

    private IEnumerable<string> BuildTribeDataString() =>
        TribesProgression.Values.Select(tribeType => tribeType.ToString()).ToArray();

    public string GetPortalData()
    {
        var sb = new SeparatedStringBuilder(PortalJoiner);

        sb.Append(SpawnPositionX);
        sb.Append(SpawnPositionY);
        sb.Append(SpawnOnBackPlane ? 1 : 0);

        return sb.ToString();
    }
}
