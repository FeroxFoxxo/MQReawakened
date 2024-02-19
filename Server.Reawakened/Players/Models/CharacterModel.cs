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
    public Dictionary<DailiesModel, DateTime> CurrentCollectedDailies { get; set; }

    public CharacterModel()
    {
        CollectedIdols = [];
        Emails = [];
        EmailMessages = [];
        Events = [];
        AchievementObjectives = [];
        BestMinigameTimes = [];

        Data = new CharacterDataModel();
        LevelData = new LevelData();
    }

    public bool CanActivateDailies(Player player, DailiesModel daily)
    {
        if (player.Character.CurrentCollectedDailies == null)
            player.Character.CurrentCollectedDailies = new Dictionary<DailiesModel, DateTime>();

        foreach (var dailyHarvest in player.Character.CurrentCollectedDailies)
        {
            if (dailyHarvest.Key.GameObjectId == daily.GameObjectId && dailyHarvest.Key.LevelId == daily.LevelId)
                return false;

            else
                return true;
        }

        player.Character.CurrentCollectedDailies.TryGetValue(daily, out var timeOfHarvest);
        var timeForNextHarvest = timeOfHarvest + TimeSpan.FromDays(1);

        return DateTime.Now >= timeForNextHarvest;
    }

    public DailiesModel GetDailyHarvest(string gameObjectId, int levelId) => new()
    {
        GameObjectId = gameObjectId,
        LevelId = levelId
    };

    public override string ToString() => throw new InvalidOperationException();
}
