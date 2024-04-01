using A2m.Server;
using Server.Reawakened.Players.Models;

namespace Server.Reawakened.Players.Extensions;

public static class CharacterExtensions
{
    public static int GetHealthForLevel(int level) => GameFlow.StatisticData.GetValue(ItemEffectType.IncreaseHitPoints, WorldStatisticsGroup.Player, level);

    public static int GetReputationForLevel(int level) => (Convert.ToInt32(Math.Pow(level, 2)) - (level - 1)) * 500;

    public static void SetLevelXp(this CharacterModel characterData, int level)
    {
        characterData.Data.GlobalLevel = level;

        if (characterData.Data.Reputation < characterData.Data.ReputationForCurrentLevel)
            characterData.Data.Reputation = characterData.Data.ReputationForCurrentLevel;

        characterData.Data.ReputationForCurrentLevel = GetReputationForLevel(level);
        characterData.Data.ReputationForNextLevel = GetReputationForLevel(level + 1);

        characterData.Data.MaxLife = GetHealthForLevel(level);
        characterData.Data.CurrentLife = characterData.Data.MaxLife;
    }

    public static bool HasAddedDiscoveredTribe(this CharacterModel characterData, TribeType tribe)
    {
        if (characterData == null)
            return false;

        if (characterData.Data.TribesDiscovered.TryGetValue(tribe, out var value))
        {
            if (value)
                return false;

            characterData.Data.TribesDiscovered[tribe] = true;
        }
        else
        {
            characterData.Data.TribesDiscovered.Add(tribe, true);
        }

        return true;
    }

    public static void ForceSetLevel(this CharacterModel characterData, int levelId, string spawnId = "")
    {
        characterData.LevelData.LevelId = levelId;
        characterData.LevelData.SpawnPointId = spawnId;
    }
}
