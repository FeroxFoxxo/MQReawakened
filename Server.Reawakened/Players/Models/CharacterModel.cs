using Server.Base.Core.Models;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Models.System;

namespace Server.Reawakened.Players.Models;

public class CharacterModel : PersistantData
{
    public CharacterDataModel Data { get; set; }
    public LevelData LevelData { get; set; }
    public Dictionary<int, List<int>> CollectedIdols { get; set; }
    public List<EmailHeaderModel> Emails { get; set; }
    public List<EmailMessageModel> EmailMessages { get; set; }
    public List<int> Events { get; set; }
    public Dictionary<int, Dictionary<string, int>> AchievementObjectives { get; set; }
    public Dictionary<string, float> BestMinigameTimes { get; set; }
    public Dictionary<string, DailiesModel> CurrentCollectedDailies { get; set; }

    public CharacterModel()
    {
        CollectedIdols = [];
        Emails = [];
        EmailMessages = [];
        Events = [];
        AchievementObjectives = [];
        BestMinigameTimes = [];
        CurrentCollectedDailies = [];

        Data = new CharacterDataModel();
        LevelData = new LevelData();
    }

    public bool CanActivateDailies(Player player, string dailyObjectId)
    {
        if (player.Character.CurrentCollectedDailies == null)
            player.Character.CurrentCollectedDailies = new Dictionary<string, DailiesModel>();

        return !player.Character.CurrentCollectedDailies.ContainsKey(dailyObjectId) ||
            player.Character.CurrentCollectedDailies.Values.Any(x => x.GameObjectId == dailyObjectId &&
                x.LevelId == player.Room.LevelInfo.LevelId && DateTime.Now >= x.TimeOfHarvest + TimeSpan.FromDays(1));
    }

    public DailiesModel SetDailyHarvest(string gameObjectId, int levelId, DateTime timeOfHarvest) => new()
    {
        GameObjectId = gameObjectId,
        LevelId = levelId,
        TimeOfHarvest = timeOfHarvest
    };

    public override string ToString() => throw new InvalidOperationException();
}
