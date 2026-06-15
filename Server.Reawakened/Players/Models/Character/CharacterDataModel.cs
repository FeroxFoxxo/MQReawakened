using A2m.Server;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Server.Reawakened.Players.Models.Character;

public class CharacterDataModel(CharacterDbEntry entry, GameVersion version) : CharacterLightModel(entry, version)
{
    private Player _player;

    public InventoryModel Inventory => new(Write);
    public HotbarModel Hotbar => new(Write);

    public StatusEffectsModel StatusEffects => new(Write);
    public CharacterResistancesModel Resistances => new(Write);
    public RecipeListModel RecipeList => new(Write);

    private PlayerListModel FriendModels =>
        new([.. Friends.Select(f => new CharacterRelationshipModel(f, _player))]);

    private PlayerListModel BlockModels =>
        new([.. Blocked.Select(b => new CharacterRelationshipModel(b, _player))]);

    private int ChatLevel => _player?.UserInfo.ChatLevel ?? 0;

    private Dictionary<int, int> IdolCount =>
        _player?.Character.CollectedIdols
            .ToDictionary(x => x.Key, x => x.Value.Count)
        ?? [];

    public List<QuestStatusModel> QuestLog => Write.QuestLog;
    public List<int> CompletedQuests => Write.CompletedQuests;
    public bool PetAutonomous => Write.PetAutonomous;
    public long GuestPassExpiry => Write.GuestPassExpiry;
    public bool ShouldExpireGuestPass => Write.ShouldExpireGuestPass;
    public Dictionary<TribeType, bool> TribesDiscovered => Write.TribesDiscovered;
    public Dictionary<TribeType, TribeDataModel> TribesProgression => Write.TribesProgression;
    public List<int> Friends => Write.Friends;
    public List<int> Blocked => Write.Blocked;
    public List<int> Muted => Write.Muted;
    public float Cash => Write.Cash;
    public float NCash => Write.NCash;
    public int ActiveQuestId => Write.ActiveQuestId;
    public int Reputation => Write.Reputation;
    public int ReputationForCurrentLevel => Write.ReputationForCurrentLevel;
    public int ReputationForNextLevel => Write.ReputationForNextLevel;
    public float SpawnPositionX => Write.SpawnPositionX;
    public float SpawnPositionY => Write.SpawnPositionY;
    public bool SpawnOnBackPlane => Write.SpawnOnBackPlane;
    public int BadgePoints => Write.BadgePoints;
    public int AbilityPower => Write.AbilityPower;
    public Dictionary<string, ReportModel> Reports => Write.Reports;
    public int Tokens => Write.Tokens;

    public void SetPlayerData(Player player)
    {
        _player = player;
        _player.NetState.Identifier = CharacterName;
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

        if (Version >= GameVersion.vPets2012)
            sb.Append(PetItemId);

        if (Version >= GameVersion.vLate2012)
            sb.Append(PetAutonomous ? 1 : 0);

        if (Version >= GameVersion.vMinigames2012)
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

        sb.Append(Id);
        sb.Append(CharacterName);
        sb.Append(Gender);
        sb.Append(Math.Floor(Cash));
        sb.Append(Math.Floor(NCash));
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
        [.. TribesProgression.Values.Select(tribeType => tribeType.ToString())];

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
        var defense = GameFlow.StatisticData.GetValue(ItemEffectType.Defence, WorldStatisticsGroup.Player, _player.Character.GlobalLevel);
        var itemList = new List<ItemDescription>();

        var defenseType = ItemEffectType.Defence;
        var tribe = TribeType.Unknown;
        switch (effect)
        {
            case ItemEffectType.FireDamage:
                defenseType = ItemEffectType.ResistFire;
                tribe = TribeType.Outlaw;
                break;
            case ItemEffectType.EarthDamage:
                defenseType = ItemEffectType.ResistEarth;
                tribe = TribeType.Bone;
                break;
            case ItemEffectType.AirDamage:
                defenseType = ItemEffectType.ResistAir;
                tribe = TribeType.Shadow;
                break;
            case ItemEffectType.IceDamage:
                defenseType = ItemEffectType.ResistIce;
                tribe = TribeType.Wild;
                break;
            case ItemEffectType.PoisonDamage:
                defenseType = ItemEffectType.ResistEarth;
                tribe = TribeType.Bone;
                break;
        }

        var progression = TribesProgression.TryGetValue(tribe, out var badgeType)
        ? badgeType.BadgePoints
        : 0;

        foreach (var item in Equipment.EquippedItems)
            itemList.Add(itemCatalog.GetItemFromId(item.Value));

        defense += statManager.ComputeEquimentBoost(defenseType, itemList) + GameFlow.StatisticData.GetValue(effect, WorldStatisticsGroup.Badge, progression / 5);

        return defense;
    }

    public int CalculateDamage(ItemDescription usedItem, ItemCatalog itemCatalog)
    {
        var statManager = new CharacterStatsManager(CharacterName);
        var itemList = new List<ItemDescription> { usedItem };

        // For blunt damage
        var effect = ItemEffectType.Unknown;
        var tribe = TribeType.Unknown;

        if (usedItem != null)
        {
            switch (usedItem.Elemental)
            {
                case Elemental.Fire:
                    effect = ItemEffectType.FireDamage;
                    tribe = TribeType.Outlaw;
                    break;
                case Elemental.Earth:
                    effect = ItemEffectType.EarthDamage;
                    tribe = TribeType.Bone;
                    break;
                case Elemental.Air:
                    effect = ItemEffectType.AirDamage;
                    tribe = TribeType.Shadow;
                    break;
                case Elemental.Ice:
                    effect = ItemEffectType.IceDamage;
                    tribe = TribeType.Wild;
                    break;
                case Elemental.Poison:
                    effect = ItemEffectType.EarthDamage;
                    tribe = TribeType.Bone;
                    break;
            }
        }

        var progression = TribesProgression.TryGetValue(tribe, out var badgeType)
        ? badgeType.BadgePoints
        : 0;

        foreach (var item in Equipment.EquippedItems)
            itemList.Add(itemCatalog.GetItemFromId(item.Value));

        var damage = statManager.ComputeEquimentBoost(ItemEffectType.BluntDamage, itemList);
        if (effect != ItemEffectType.Unknown)
            damage = (int)(damage * ((100 + GameFlow.StatisticData.GetGlobalStat(Globals.ElementalDamageRatio)) / 100));

        damage += GameFlow.StatisticData.GetValue(ItemEffectType.AbilityPower, WorldStatisticsGroup.Player, _player.Character.GlobalLevel);

        damage += GameFlow.StatisticData.GetValue(effect, WorldStatisticsGroup.Badge, progression / 5);

        return damage;
    }

    public PlayerListModel GetFriends() => FriendModels;

    public PlayerListModel GetBlocked() => BlockModels;
}
