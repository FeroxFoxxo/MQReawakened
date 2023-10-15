using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Players.Extensions;

public static class PlayerExtensions
{
    public static void RemoveFromGroup(this Player player)
    {
        if (player.Group == null)
            return;

        player.Group.GroupMembers.Remove(player);

        if (player.Group.GroupMembers.Count > 0)
        {
            if (player.Group.LeaderCharacterName == player.Character.Data.CharacterName)
            {
                var newLeader = player.Group.GroupMembers.First();
                player.Group.LeaderCharacterName = newLeader.Character.Data.CharacterName;

                foreach (var member in player.Group.GroupMembers)
                    member.SendXt("pp", player.Group.LeaderCharacterName);
            }

            foreach (var member in player.Group.GroupMembers)
                member.SendXt("pl", player.Character.Data.CharacterName);
        }

        player.Group = null;
    }

    public static void AddReputation(this Player player, int reputation)
    {
        var charData = player.Character.Data;

        reputation += charData.Reputation;

        while (reputation > charData.ReputationForNextLevel)
        {
            reputation -= charData.ReputationForNextLevel;
            player.Character.SetLevelXp(charData.GlobalLevel + 1, reputation);
            player.SendLevelUp();
        }

        charData.Reputation = reputation;

        player.SendXt("cp", charData.Reputation, charData.ReputationForNextLevel);
    }

    public static void AddBananas(this Player player, int collectedBananas)
    {
        var charData = player.Character.Data;
        charData.Cash += collectedBananas;
        player.SendCashUpdate();
    }

    public static void RemoveBananas(this Player player, int collectedBananas)
    {
        var charData = player.Character.Data;
        charData.Cash -= collectedBananas;
        player.SendCashUpdate();
    }

     public static void AddNCash(this Player player, int collectedNCash)
    {
        var charData = player.Character.Data;
        charData.NCash += collectedNCash;
        player.SendCashUpdate();
    }

    public static void RemoveNCash(this Player player, int collectedNCash)
    {
        var charData = player.Character.Data;
        charData.NCash -= collectedNCash;
        player.SendCashUpdate();
    }

    public static void AddPoints(this Player player, int abilityPoints)
    {
        var charData = player.Character.Data;
        charData.BadgePoints += abilityPoints;
        player.SendPointsUpdate();
    }

    public static void SendCashUpdate(this Player player)
    {
        var charData = player.Character.Data;
        player.SendXt("ca", charData.Cash, charData.NCash);
    }

    public static void SendPointsUpdate(this Player player)
    {
        var charData = player.Character.Data;
        player.SendXt("ca", charData.BadgePoints);
    }

    public static void SendLevelChange(this Player player, WorldHandler worldHandler, WorldGraphXML worldGraph)
    {
        var error = string.Empty;
        var levelName = string.Empty;
        var surroundingLevels = string.Empty;

        try
        {
            var levelInfo = worldHandler.GetLevelInfo(player.GetLevelId());
            levelName = levelInfo.Name;

            var sb = new SeparatedStringBuilder('!');

            var levels = worldGraph.GetLevelWorldGraphNodes(levelInfo.LevelId)
                .Where(x => x.ToLevelID != x.LevelID)
                .Select(x => worldGraph.GetInfoLevel(x.ToLevelID).Name)
                .Distinct();

            foreach (var level in levels)
                sb.Append(level);

            surroundingLevels = sb.ToString();
        }
        catch (Exception e)
        {
            error = e.Message;
        }

        player.SendXt("lw", error, levelName, surroundingLevels);
    }

    public static CharacterModel GetCharacterFromName(this Player player, string characterName)
        => player.UserInfo.Characters.Values
            .FirstOrDefault(c => c.Data.CharacterName == characterName);

    public static void SetCharacterSelected(this Player player, int characterId)
    {
        player.Character = player.UserInfo.Characters[characterId];
        player.UserInfo.LastCharacterSelected = player.Character.Data.CharacterName;
    }

    public static void AddCharacter(this Player player, CharacterModel character) =>
        player.UserInfo.Characters.Add(character.Data.CharacterId, character);

    public static void DeleteCharacter(this Player player, int id)
    {
        player.UserInfo.Characters.Remove(id);

        player.UserInfo.LastCharacterSelected = player.UserInfo.Characters.Count > 0
            ? player.UserInfo.Characters.First().Value.Data.CharacterName
            : string.Empty;
    }

    public static void LevelUp(this Player player, int level, Microsoft.Extensions.Logging.ILogger logger)
    {
        player.Character.SetLevelXp(level);
        player.SendLevelUp();

        logger.LogTrace("{Name} leveled up to {Level}", player.Character.Data.CharacterName, level);
    }

    public static void DiscoverTribe(this Player player, TribeType tribe)
    {
        if (player.Character.HasAddedDiscoveredTribe(tribe))
            player.SendXt("cB", (int)tribe);
    }

    public static void DiscoverAllTribes(this Player player)
    {
        foreach (TribeType tribe in Enum.GetValues(typeof(TribeType)))
            player.DiscoverTribe(tribe);
    }

    public static void AddSlots(this Player player, bool hasPet)
    {
        var hotbarButtons = player.Character.Data.Hotbar.HotbarButtons;

        for (var i = 0; i < (hasPet ? 5 : 4); i++)
        {
            if (!hotbarButtons.ContainsKey(i))
            {
                var itemModel = new ItemModel()
                {
                    ItemId = 340,
                    Count = 1,
                    BindingCount = 1,
                    DelayUseExpiry = DateTime.MinValue
                };
                hotbarButtons[i] = itemModel;
            }
        }
    }

    public static void CheckObjective(this Player player, QuestCatalog quests,
        ObjectiveEnum type, int gameObjectId, int itemId, int count)
    {
        var character = player.Character.Data;

        foreach (var quest in character.QuestLog)
        {
            foreach (var objectiveKVP in quest.Objectives)
            {
                var objective = objectiveKVP.Value;
                var hasObjComplete = false;

                if (objective.ObjectiveType != type ||
                    objective.GameObjectId != gameObjectId ||
                    objective.ItemId != itemId ||
                    objective.Order > quest.CurrentOrder ||
                    objective.LevelId != player.Character.LevelData.LevelId ||
                    objective.Completed ||
                    count <= 0)
                    continue;

                objective.CountLeft -= count;

                player.SendXt("nu", quest.Id, objectiveKVP.Key, objective.CountLeft);

                if (objective.CountLeft <= 0)
                {
                    objective.CountLeft = 0;
                    objective.Completed = true;

                    var leftObjectives = quest.Objectives.Where(x => x.Value.Completed != true);

                    if (leftObjectives.Any())
                        quest.CurrentOrder = leftObjectives.Min(x => x.Value.Order);

                    player.SendXt("no", quest.Id, objectiveKVP.Key);
                    hasObjComplete = true;
                }

                if (!quest.Objectives.Any(o => !o.Value.Completed) && hasObjComplete)
                {
                    player.SendXt("nQ", quest.Id);
                    quest.QuestStatus = QuestStatus.QuestState.TO_BE_VALIDATED;
                    player.UpdateNpcsInLevel(quest, quests);
                    return;
                }
            }
        }
    }
}
