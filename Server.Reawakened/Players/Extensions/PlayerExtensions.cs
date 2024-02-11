using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Rooms.Extensions;
using static A2m.Server.QuestStatus;

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

        if (player.TempData.ReputationBoostsElixir)
            reputation = Convert.ToInt32(reputation * 0.1);

        charData.Reputation = reputation;
        player.SendXt("cp", charData.Reputation, charData.ReputationForNextLevel);
    }

    public static void TradeWithPlayer(this Player origin)
    {
        var tradeModel = origin.TempData.TradeModel;
        var catalog = origin.DatabaseContainer.ItemCatalog;

        if (tradeModel == null)
            return;

        var tradingPlayer = tradeModel.TradingPlayer;

        foreach (var item in tradeModel.ItemsInTrade)
        {
            var itemDesc = catalog.GetItemFromId(item.Key);

            tradeModel.TradingPlayer.AddItem(itemDesc, item.Value);
            origin.RemoveItem(itemDesc, item.Value);
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

        if (player.TempData.BananaBoostsElixir)
            collectedBananas = Convert.ToInt32(collectedBananas * 0.1);

        charData.Cash += collectedBananas;
        player.SendCashUpdate();
    }

    public static void RemoveBananas(this Player player, int collectedBananas)
    {
        var charData = player.Character.Data;
        charData.Cash -= collectedBananas;

        if (charData.Cash < 0)
            charData.Cash = 0;

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

        if (charData.NCash < 0)
            charData.NCash = 0;

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

    public static void SendLevelChange(this Player player)
    {
        var worldHandler = player.DatabaseContainer.WorldHandler;

        var error = string.Empty;
        var levelName = string.Empty;
        var surroundingLevels = string.Empty;

        try
        {
            var levelInfo = worldHandler.GetLevelInfo(player.GetLevelId());
            levelName = levelInfo.Name;

            var sb = new SeparatedStringBuilder('!');

            var levels = worldHandler.GetSurroundingLevels(levelInfo);

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

    public static void SetCharacterSelected(this Player player, CharacterModel character)
    {
        player.Character = character;
        player.UserInfo.LastCharacterSelected = player.CharacterName;
    }

    public static void AddCharacter(this Player player, CharacterModel character) =>
        player.UserInfo.CharacterIds.Add(character.Id);

    public static void DeleteCharacter(this Player player, int id)
    {
        var characterHandler = player.DatabaseContainer.CharacterHandler;

        player.UserInfo.CharacterIds.Remove(id);

        characterHandler.Data.Remove(id);

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

    public static void SetObjective(this Player player, ObjectiveEnum type, string gameObjectId, string prefabName, int count) =>
        player.CheckObjective(type, gameObjectId, prefabName, count, true);

    public static void CheckObjective(this Player player, ObjectiveEnum type, string gameObjectId, string prefabName, int count, bool setObjective = false)
    {
        if (count <= 0)
            return;

        var character = player.Character.Data;

        player.Room.Logger.LogDebug("Checking {type} objective for {prefab} id ({id}) of count {count}.", type, prefabName, gameObjectId, count);

        foreach (var quest in character.QuestLog)
        {
            var hasObjComplete = false;

            foreach (var objectiveKVP in quest.Objectives)
            {
                var objective = objectiveKVP.Value;

                if (objective.ObjectiveType != type || objective.Completed)
                    continue;

                var meetsRequirement = false;

                if (objective.GameObjectId > 0)
                    if (objective.GameObjectId.ToString() == gameObjectId &&
                        objective.LevelId == player.Character.LevelData.LevelId)
                        meetsRequirement = true;

                if (objective.ItemId > 0 && !meetsRequirement)
                {
                    var item = player.DatabaseContainer.ItemCatalog.GetItemFromPrefabName(prefabName);

                    if (item != null)
                        if (item.ItemId == objective.ItemId)
                            meetsRequirement = item.InventoryCategoryID is not ItemFilterCategory.None
                                                                        and not ItemFilterCategory.QuestItems
                                               || objective.LevelId == player.Character.LevelData.LevelId;
                }

                if (!meetsRequirement)
                    continue;

                if (setObjective)
                    objective.CountLeft = objective.Total - count;
                else
                    objective.CountLeft -= count;

                if (objective.ObjectiveType == ObjectiveEnum.AlterandReceiveitem)
                    objective.CountLeft = 0;

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
                else
                    player.SendXt("nu", quest.Id, objectiveKVP.Key, objective.CountLeft);
            }

            if (hasObjComplete)
            {
                if (!quest.Objectives.Any(o => !o.Value.Completed))
                {
                    player.SendXt("nQ", quest.Id);
                    quest.QuestStatus = QuestState.TO_BE_VALIDATED;
                }
                player.UpdateNpcsInLevel(quest);
            }
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
