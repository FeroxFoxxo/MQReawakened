using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Services;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Server.Reawakened.Players.Extensions;

public static class PlayerExtensions
{
    public static void TeleportPlayer(this Player player, int x, int y, int z)
    {
        var isBackPlane = z == 1;

        var coordinates = new PhysicTeleport_SyncEvent(player.GameObjectId.ToString(),
            player.Room.Time, player.TempData.Position.X + x, player.TempData.Position.Y + y, isBackPlane);

        player.SendSyncEventToPlayer(coordinates);
    }

    public static void RemoveFromGroup(this Player player)
    {
        var group = player.TempData.Group;

        if (group == null)
            return;

        player.TempData.Group = null;

        foreach (var member in group.GetMembers())
            member.SendXt("pl", player.CharacterName);

        group.RemovePlayer(player);

        var members = group.GetMembers();

        if (members.Count > 0)
        {
            if (group.GetLeaderName() == player.CharacterName)
            {
                var newLeader = members.First();

                group.SetLeaderName(newLeader.CharacterName);

                foreach (var member in members)
                    member.SendXt("pp", newLeader.CharacterName);
            }

            if (members.Count == 1)
                members.First().RemoveFromGroup();
        }
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

    public static void TradeWithPlayer(this Player origin, ItemCatalog catalog)
    {
        var tradeModel = origin.TempData.TradeModel;

        if (tradeModel == null)
            return;

        var tradingPlayer = tradeModel.TradingPlayer;

        foreach (var item in tradeModel.ItemsInTrade)
        {
            var itemDesc = catalog.GetItemFromId(item.Key);

            tradeModel.TradingPlayer.Character.AddItem(itemDesc, item.Value);
            origin.Character.RemoveItem(itemDesc, item.Value);
        }

        tradingPlayer.Character.Data.Cash += tradeModel.BananasInTrade;
        origin.Character.Data.Cash -= tradeModel.BananasInTrade;
    }

    public static void RemoveTrade(this Player player)
    {
        if (player == null)
            return;

        if (player.TempData.TradeModel == null)
            return;

        var trade = player.TempData.TradeModel.TradingPlayer;

        player.TempData.TradeModel = null;

        if (trade != null)
            trade.TempData.TradeModel = null;
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

    public static void AddPoints(this Player player)
    {
        var charData = player.Character.Data;

        charData.BadgePoints = 0;
        charData.BadgePoints += 100;

        player.SendLevelUp();
    }

    public static void SendCashUpdate(this Player player)
    {
        var charData = player.Character.Data;
        player.SendXt("ca", charData.Cash, charData.NCash);
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

    public static void SetCharacterSelected(this Player player, int characterId, CharacterHandler characterHandler)
    {
        player.Character = characterHandler.Get(characterId);
        player.UserInfo.LastCharacterSelected = player.CharacterName;
    }

    public static void AddCharacter(this Player player, CharacterModel character) =>
        player.UserInfo.CharacterIds.Add(character.Id);

    public static void DeleteCharacter(this Player player, int id, CharacterHandler characterHandler)
    {
        player.UserInfo.CharacterIds.Remove(id);

        player.UserInfo.LastCharacterSelected = player.UserInfo.CharacterIds.Count > 0
            ? characterHandler.Get(player.UserInfo.CharacterIds.First()).Data.CharacterName
            : string.Empty;
    }

    public static void LevelUp(this Player player, int level, Microsoft.Extensions.Logging.ILogger logger)
    {
        player.Character.SetLevelXp(level);
        player.SendLevelUp();

        logger.LogTrace("{Name} leveled up to {Level}", player.CharacterName, level);
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

    public static void CheckObjective(this Player player, QuestCatalog quests, ObjectiveCatalogInt objCatalog,
        ObjectiveEnum type, int gameObjectId, string prefabName, int count)
    {
        if (!objCatalog.ObjectivePrefabs.TryGetValue(prefabName, out var itemIds))
            return;

        if (itemIds == null)
            return;

        var character = player.Character.Data;

        foreach (var quest in character.QuestLog)
        {
            foreach (var objectiveKVP in quest.Objectives)
            {
                var objective = objectiveKVP.Value;
                var hasObjComplete = false;

                if (objective.GameObjectId > 0)
                    if (objective.GameObjectId != gameObjectId)
                        continue;

                if (objective.ObjectiveType != type ||
                    !itemIds.Select(x => int.TryParse(x, out var id) ? id : 0).Contains(objective.ItemId) ||
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
                }
            }

            player.UpdateNpcsInLevel(quest, quests);
        }
    }

    public static void UpdateEquipment(this Player sentPlayer)
    {
        foreach (
            var player in
            from player in sentPlayer.Room.Players.Values
            select player
        )
            player.SendXt("iq", sentPlayer.UserId, sentPlayer.Character.Data.Equipment);
    }
}
