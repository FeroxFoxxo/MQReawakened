using A2m.Server;
using System.Globalization;
using System.Text;

namespace Server.Reawakened.Characters.Models;

public class CharacterDetailedModel : CharacterLightModel
{
    public const char FieldJoiner = '&';

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

    public CharacterDetailedModel() {}

    public CharacterDetailedModel(string serverData) : base(serverData)
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
        var sb = new StringBuilder();

        sb.Append(Customization);
        sb.Append(CharacterDataEndDelimiter);
        sb.Append(Inventory);
        sb.Append(CharacterDataEndDelimiter);
        foreach (var qs in QuestLog)
        {
            sb.Append(qs);
            sb.Append(FieldJoiner);
        }
        sb.Append(CharacterDataEndDelimiter);
        foreach (var completedQuest in CompletedQuests)
        {
            sb.Append(completedQuest);
            sb.Append(FieldJoiner);
        }
        sb.Append(CharacterDataEndDelimiter);
        sb.Append(Hotbar);
        sb.Append(CharacterDataEndDelimiter);
        sb.Append(FriendList);
        sb.Append(CharacterDataEndDelimiter);
        sb.Append(BlockList);
        sb.Append(CharacterDataEndDelimiter);
        sb.Append(Equipment);
        sb.Append(CharacterDataEndDelimiter);
        sb.Append(PetItemId);
        sb.Append(CharacterDataEndDelimiter);
        sb.Append(PetAutonomous ? 1 : 0);
        sb.Append(CharacterDataEndDelimiter);
        sb.Append(GuestPassExpiry);
        sb.Append(CharacterDataEndDelimiter);
        sb.Append(ShouldExpireGuestPass ? 1 : 0);
        sb.Append(CharacterDataEndDelimiter);
        sb.Append(Registered ? 1 : 0);
        sb.Append(CharacterDataEndDelimiter);
        sb.Append(Resistances);
        sb.Append(CharacterDataEndDelimiter);
        sb.Append(RecipeList);
        sb.Append(CharacterDataEndDelimiter);
        sb.Append(BuildTribesDiscoveredString(TribesDiscovered));
        sb.Append(CharacterDataEndDelimiter);
        sb.Append(BuildIdolCountString(IdolCount));
        sb.Append(CharacterDataEndDelimiter);
        sb.Append(BuildStatsDiscoveredString(DiscoveredStats));
        sb.Append(CharacterDataEndDelimiter);

        sb.Append(FieldSeparator);
        sb.Append(CharacterId);
        sb.Append(FieldSeparator);
        sb.Append(CharacterName);
        sb.Append(FieldSeparator);
        sb.Append(Gender);
        sb.Append(FieldSeparator);
        sb.Append(Cash);
        sb.Append(FieldSeparator);
        sb.Append(NCash);
        sb.Append(FieldSeparator);
        sb.Append(ActiveQuestId);
        sb.Append(FieldSeparator);
        sb.Append(MaxLife);
        sb.Append(FieldSeparator);
        sb.Append(FieldSeparator);
        sb.Append(CurrentLife);
        sb.Append(FieldSeparator);
        sb.Append(GlobalLevel);
        sb.Append(FieldSeparator);
        sb.Append(InteractionStatus);
        sb.Append(FieldSeparator);
        sb.Append(Reputation);
        sb.Append(FieldSeparator);
        sb.Append(ReputationForCurrentLevel);
        sb.Append(FieldSeparator);
        sb.Append(ReputationForNextLevel);
        sb.Append(FieldSeparator);
        sb.Append(SpawnPositionX);
        sb.Append(FieldSeparator);
        sb.Append(SpawnPositionY);
        sb.Append(FieldSeparator);
        sb.Append(SpawnOnBackPlane ? 1 : 0);
        sb.Append(FieldSeparator);
        sb.Append(BadgePoints);
        sb.Append(FieldSeparator);
        sb.Append((int)Allegiance);
        sb.Append(FieldSeparator);
        sb.Append(AbilityPower);
        sb.Append(FieldSeparator);
        sb.Append(ChatLevel);
        sb.Append(string.Join(FieldSeparator, BuildTribeDataString(TribesProgression)));

        return sb.ToString();
    }

    private static string BuildTribesDiscoveredString(Dictionary<TribeType, bool> tribesDiscovered)
    {
        var sb = new StringBuilder();

        foreach (var kvp in tribesDiscovered)
        {
            sb.Append((int)kvp.Key);
            sb.Append(FieldSeparator);
            sb.Append(kvp.Value ? "1" : "0");
            sb.Append(FieldSeparator);
        }

        return sb.ToString();
    }

    private static string BuildIdolCountString(Dictionary<int, int> idolCount)
    {
        var sb = new StringBuilder();

        foreach (var kvp in idolCount)
        {
            sb.Append(kvp.Key);
            sb.Append(FieldSeparator);
            sb.Append(kvp.Value);
            sb.Append(FieldSeparator);
        }

        return sb.ToString();
    }

    private static string BuildStatsDiscoveredString(HashSet<int> discoveredStats)
    {
        var sb = new StringBuilder();

        foreach (var stat in discoveredStats)
        {
            sb.Append(stat.ToString(CultureInfo.InvariantCulture));
            sb.Append(FieldSeparator);
        }

        return sb.ToString();
    }

    private static string[] BuildTribeDataString(Dictionary<TribeType, TribeDataModel> tribesProgression) =>
        tribesProgression.Values.Select(tribeType => tribeType.ToString()).ToArray();
}
