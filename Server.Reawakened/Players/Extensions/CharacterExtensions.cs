using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;

namespace Server.Reawakened.Players.Extensions;

public static class CharacterExtensions
{
    public static bool HasDiscoveredTribe(this CharacterModel characterData, TribeType tribe)
    {
        if (characterData == null) return false;

        // ReSharper disable once InvertIf
        if (characterData.Data.TribesDiscovered.TryGetValue(tribe, out var discovered))
            if (discovered)
                return true;

        return characterData.Data.Allegiance == tribe;
    }

    public static int GetHealthForLevel(int level) => (level - 1) * 270 + 81;

    public static int GetReputationForLevel(int level) => (Convert.ToInt32(Math.Pow(level, 2)) - (level - 1)) * 500;

    public static void SetLevelXp(this CharacterModel characterData, int level, int reputation = 0)
    {
        characterData.Data.GlobalLevel = level;

        characterData.Data.ReputationForCurrentLevel = GetReputationForLevel(level - 1);
        characterData.Data.ReputationForNextLevel = GetReputationForLevel(level);
        characterData.Data.Reputation = reputation;

        characterData.Data.MaxLife = GetHealthForLevel(level);
        characterData.Data.CurrentLife = characterData.Data.MaxLife;
    }

    public static bool HasAddedDiscoveredTribe(this CharacterModel characterData, TribeType tribe)
    {
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

    public static void SetLevel(this CharacterModel character, int levelId,
        Microsoft.Extensions.Logging.ILogger logger) =>
        character.SetLevel(levelId, 0, 0, logger);

    public static void SetLevel(this CharacterModel character, int levelId, int portalId, int spawnId,
        Microsoft.Extensions.Logging.ILogger logger)
    {
        character.LevelData.LevelId = levelId;
        character.LevelData.PortalId = portalId;
        character.LevelData.SpawnPointId = spawnId;

        logger.LogDebug("Set spawn of '{CharacterName}' to portal {PortalId} spawn {SpawnId}",
            character.Data.CharacterName,
            portalId, spawnId);
    }

    public static void AddQuest(this CharacterModel character, QuestDescription quest, bool setActive)
    {
        if (character.HasQuest(quest.Id)) return;

        character.Data.QuestLog.Add(new QuestStatusModel
        {
            QuestStatus = QuestStatus.QuestState.IN_PROCESSING,
            Id = quest.Id,
            Objectives = quest.Objectives.ToDictionary(
                x => x.Key,
                x => new ObjectiveModel
                {
                    Completed = false,
                    CountLeft = x.Value.TotalCount
                }
            )
        });

        if (setActive)
            character.Data.ActiveQuestId = quest.Id;
    }

    public static bool HasQuest(this CharacterModel character, int questId) =>
        character.Data.QuestLog.Count != 0 && character.Data.QuestLog.Any(quest => quest.Id == questId);

    public static bool TryGetQuest(this CharacterModel model, int questId, out QuestStatusModel outQuest)
    {
        outQuest = null;

        if (model.Data.QuestLog.Count == 0)
            return false;

        foreach (var quest in model.Data.QuestLog.Where(quest => quest.Id == questId))
        {
            outQuest = quest;
            return true;
        }

        return false;
    }

    public static bool HasPreviousQuests(this CharacterModel model, QuestDescription quest) =>
        model.Data.CompletedQuests.Count != 0 && quest.PreviousQuests.Where(prevId => prevId.Key != 0)
            .All(prevId => model.Data.CompletedQuests.Contains(prevId.Key));
}
