using A2m.Server;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Database.Characters;

namespace Server.Reawakened.Players.Extensions;

public static class CharacterExtensions
{
    public static int GetHealthForLevel(int level) => GameFlow.StatisticData.GetValue(ItemEffectType.IncreaseHitPoints, WorldStatisticsGroup.Player, level);

    public static int GetReputationForLevel(int level)
    {
        var currentLevel = Convert.ToInt32(Math.Pow(level, 2));
        var lastLevel = Math.Max(0, level - 1);

        return (currentLevel - lastLevel) * 500;
    }

    public static void SetLevelXp(this CharacterModel characterData, int level, ServerRConfig maxConfig)
    {
        if (level > maxConfig.MaxLevel)
            level = maxConfig.MaxLevel;

        if (level < 1)
            level = 1;

        characterData.Write.GlobalLevel = level;

        if (characterData.Reputation < characterData.ReputationForCurrentLevel)
            characterData.Write.Reputation = characterData.ReputationForCurrentLevel;

        characterData.Write.ReputationForCurrentLevel = GetReputationForLevel(level - 1);
        characterData.Write.ReputationForNextLevel = GetReputationForLevel(level);

        characterData.Write.MaxLife = GetHealthForLevel(level);
        characterData.Write.CurrentLife = characterData.MaxLife;
    }

    public static bool HasAddedDiscoveredTribe(this CharacterModel characterData, TribeType tribe)
    {
        if (characterData == null)
            return false;

        if (characterData.TribesDiscovered.TryGetValue(tribe, out var value))
        {
            if (value)
                return false;

            characterData.TribesDiscovered[tribe] = true;
        }
        else
        {
            characterData.TribesDiscovered.Add(tribe, true);
        }

        return true;
    }

    public static void ForceSetLevel(this CharacterModel characterData, int levelId, string spawnId = "")
    {
        characterData.Write.LevelId = levelId;
        characterData.Write.SpawnPointId = spawnId;
    }
}
