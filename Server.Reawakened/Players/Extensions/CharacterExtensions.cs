using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Network;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;
using WorldGraphDefines;
using static LeaderBoardTopScoresJson;

namespace Server.Reawakened.Players.Extensions;

public static class CharacterExtensions
{
    public static bool HasDiscoveredTribe(this CharacterDataModel characterData, TribeType tribe)
    {
        if (characterData == null) return false;

        if (characterData.TribesDiscovered.TryGetValue(tribe, out var discovered))
            if (discovered)
                return true;

        return characterData.Allegiance == tribe;
    }

    public static CharacterModel GetCurrentCharacter(this Player player)
        => player.UserInfo.Characters[player.CurrentCharacter];

    public static CharacterModel GetCharacterFromName(this Player player, string characterName)
        => player.UserInfo.Characters.Values
            .FirstOrDefault(c => c.Data.CharacterName == characterName);

    public static void SetCharacterSelected(this Player player, int characterId)
    {
        player.CurrentCharacter = characterId;
        player.UserInfo.LastCharacterSelected = player.GetCurrentCharacter().Data.CharacterName;
    }

    public static void AddCharacter(this Player player, CharacterModel characterData) =>
        player.UserInfo.Characters.Add(characterData.Data.CharacterId, characterData);

    public static void DeleteCharacter(this Player player, int id)
    {
        player.UserInfo.Characters.Remove(id);

        player.UserInfo.LastCharacterSelected = player.UserInfo.Characters.Count > 0
            ? player.UserInfo.Characters.First().Value.Data.CharacterName
            : string.Empty;
    }

    public static void LevelUp(this CharacterDataModel characterData, int level)
    {
        characterData.GlobalLevel = level;

        characterData.ReputationForCurrentLevel = GetReputationForLevel(level - 1);
        characterData.ReputationForNextLevel = GetReputationForLevel(level);
        characterData.Reputation = 0;

        characterData.MaxLife = GetHealthForLevel(level);
    }

    private static int GetHealthForLevel(int level) => (level - 1) * 270 + 81;

    private static int GetReputationForLevel(int level) => (Convert.ToInt32(Math.Pow(level, 2)) - (level - 1)) * 500;
    
    public static void DiscoverTribe(this NetState state, TribeType tribe)
    {
        var player = state.Get<Player>();
        var character = player.GetCurrentCharacter();

        if (HasAddedDiscoveredTribe(character.Data, tribe))
            state.SendXt("cB", (int)tribe);
    }

    public static bool HasAddedDiscoveredTribe(this CharacterDataModel model, TribeType tribe)
    {
        if (model.TribesDiscovered.ContainsKey(tribe))
        {
            if (model.TribesDiscovered[tribe])
                return false;

            model.TribesDiscovered[tribe] = true;
        }
        else
        {
            model.TribesDiscovered.Add(tribe, true);
        }
        return true;
    }

    public static void AddBananas(this Player player, NetState state, int collectedBananas)
    {
        var charData = player.GetCurrentCharacter().Data;
        charData.Cash += collectedBananas;
        state.SendXt("ca", charData.Cash, charData.NCash);
    }

    public static void SendLevelChange(this Player player, NetState netState, LevelHandler levelHandler,
        WorldGraphXML worldGraph)
    {
        var error = string.Empty;
        var levelName = string.Empty;
        var surroundingLevels = string.Empty;

        try
        {
            var level = player.GetCurrentLevel(levelHandler);

            levelName = level.LevelInfo.Name;
            surroundingLevels = GetSurroundingLevels(level.LevelInfo, worldGraph);
        }
        catch (Exception e)
        {
            error = e.Message;
        }

        netState.SendXt("lw", error, levelName, surroundingLevels);
    }

    private static string GetSurroundingLevels(LevelInfo levelInfo, WorldGraphXML worldGraph)
    {
        var sb = new SeparatedStringBuilder('!');

        var levels = worldGraph.GetLevelWorldGraphNodes(levelInfo.LevelId)
            .Where(x => x.ToLevelID != x.LevelID)
            .Select(x => worldGraph.GetInfoLevel(x.ToLevelID).Name)
            .Distinct();

        foreach (var level in levels)
            sb.Append(level);

        return sb.ToString();
    }

    public static void SetCharacterSpawn(this CharacterModel model, int portalId, int spawnId,
        Microsoft.Extensions.Logging.ILogger logger)
    {
        model.PortalId = portalId;
        model.SpawnPoint = spawnId;
        logger.LogDebug("Set spawn of '{CharacterName}' to portal {PortalId} spawn {SpawnId}", model.Data.CharacterName,
            portalId, spawnId);
    }

    public static void AddQuest(this CharacterModel model, QuestDescription quest, bool setActive)
    {
        if (model.HasQuest(quest.Id)) return;

        model.Data.QuestLog.Add(new QuestStatusModel
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
            model.Data.ActiveQuestId = quest.Id;
    }

    public static bool HasQuest(this CharacterModel model, int questId)
    {
        if (model.Data.QuestLog.Count == 0) return false;

        foreach (var quest in model.Data.QuestLog)
            if (quest.Id == questId)
                return true;

        return false;
    }

    public static bool TryGetQuest(this CharacterModel model, int questId, out QuestStatusModel outQuest)
    {
        outQuest = null;
        if (model.Data.QuestLog.Count == 0) return false;

        foreach (var quest in model.Data.QuestLog)
        {
            if (quest.Id == questId)
            {
                outQuest = quest;
                return true;
            }
        }

        return false;
    }

    public static bool HasPreviousQuests(this CharacterModel model, QuestDescription quest)
    {
        if (model.Data.CompletedQuests.Count == 0) return false;

        foreach (var prevId in quest.PreviousQuests)
        {
            if (prevId.Key == 0) continue;
            if (!model.Data.CompletedQuests.Contains(prevId.Key))
                return false;
        }

        return true;
    }
}
