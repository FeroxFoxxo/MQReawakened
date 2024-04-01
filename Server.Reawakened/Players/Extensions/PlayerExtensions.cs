using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Services;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using Server.Reawakened.XMLs.Enums;
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

    public static void AddReputation(this Player player, int reputation, ServerRConfig config)
    {
        if (player == null)
            return;

        var charData = player.Character.Data;

        if (player.TempData.ReputationBoostsElixir)
            reputation = Convert.ToInt32(reputation * 0.1);

        reputation += charData.Reputation;

        while (reputation > charData.ReputationForNextLevel)
        {
            player.Character.SetLevelXp(charData.GlobalLevel + 1);
            player.SendLevelUp();
        }

        charData.Reputation = reputation;

        if (config.GameVersion == GameVersion.v2014)
            player.SendXt("cp", charData.Reputation, charData.ReputationForNextLevel);

        else
            player.SendXt("cp", charData.Reputation - charData.ReputationForCurrentLevel,
                charData.ReputationForNextLevel - charData.ReputationForCurrentLevel);
    }

    public static void TradeWithPlayer(this Player origin, ItemCatalog itemCatalog)
    {
        var tradeModel = origin.TempData.TradeModel;

        if (tradeModel == null)
            return;

        var tradingPlayer = tradeModel.TradingPlayer;

        foreach (var item in tradeModel.ItemsInTrade)
        {
            var itemDesc = itemCatalog.GetItemFromId(item.Key);

            tradeModel.TradingPlayer.AddItem(itemDesc, item.Value, itemCatalog);
            origin.RemoveItem(itemDesc, item.Value, itemCatalog);
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

    public static void AddBananas(this Player player, int collectedBananas, InternalAchievement internalAchievement, Microsoft.Extensions.Logging.ILogger logger)
    {
        var charData = player.Character.Data;

        if (player.TempData.BananaBoostsElixir)
            collectedBananas = Convert.ToInt32(collectedBananas * 0.1);

        player.CheckAchievement(AchConditionType.CollectBanana, string.Empty, internalAchievement, logger, collectedBananas);

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

    public static void SendLevelChange(this Player player, WorldHandler worldHandler)
    {
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

    public static void DeleteCharacter(this Player player, int id, CharacterHandler characterHandler)
    {
        player.UserInfo.CharacterIds.Remove(id);

        characterHandler.Remove(id);

        player.UserInfo.LastCharacterSelected = player.UserInfo.CharacterIds.Count > 0
            ? characterHandler.Get(player.UserInfo.CharacterIds.First()).Data.CharacterName
            : string.Empty;
    }

    public static void LevelUp(this Player player, int level, Microsoft.Extensions.Logging.ILogger logger)
    {
        player.Character.SetLevelXp(level);
        player.SendLevelUp();

        player.AddNCash(125); //Temporary way to earn NC upon level up.
        player.SendCashUpdate();
        //(Needed for gameplay improvements as NC is currently unobtainable)

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

    public static void CheckObjective(this Player player, ObjectiveEnum type, string gameObjectId, string prefabName,
        int count, QuestCatalog questCatalog) => player.CheckObjective(type, gameObjectId, prefabName, count, questCatalog, questCatalog.ItemCatalog);

    public static void CheckObjective(this Player player, ObjectiveEnum type, string gameObjectId, string prefabName,
        int count, ItemCatalog itemCatalog) => player.CheckObjective(type, gameObjectId, prefabName, count, itemCatalog.QuestCatalog, itemCatalog);

    public static void SetObjective(this Player player, ObjectiveEnum type, string gameObjectId, string prefabName,
        int count, QuestCatalog questCatalog) => player.CheckObjective(type, gameObjectId, prefabName, count, questCatalog, questCatalog.ItemCatalog, true);

    public static void SetObjective(this Player player, ObjectiveEnum type, string gameObjectId, string prefabName,
        int count, ItemCatalog itemCatalog) => player.CheckObjective(type, gameObjectId, prefabName, count, itemCatalog.QuestCatalog, itemCatalog, true);

    private static void CheckObjective(this Player player, ObjectiveEnum type, string gameObjectId, string prefabName,
        int count, QuestCatalog questCatalog, ItemCatalog itemCatalog, bool setObjective = false)
    {
        if (count <= 0)
            return;

        if (player == null)
            return;

        if (player.Character == null || player.Room == null)
            return;

        var character = player.Character.Data;

        player.Room.Logger.LogDebug("Checking {type} objective for {prefab} id ({id}) of count {count}.", type, prefabName, gameObjectId, count);

        foreach (var quest in character.QuestLog)
        {
            var hasObjComplete = false;

            foreach (var objectiveKVP in quest.Objectives)
            {
                var objective = objectiveKVP.Value;

                if (objective == null)
                    continue;

                if (objective.ObjectiveType != type || objective.Completed)
                    continue;

                var meetsRequirement = false;

                if (objective.GameObjectId > 0)
                    if (objective.GameObjectId.ToString() == gameObjectId &&
                        objective.LevelId == player.Character.LevelData.LevelId)
                        meetsRequirement = true;

                if (objective.ItemId > 0 && !meetsRequirement)
                {
                    var item = itemCatalog.GetItemFromPrefabName(prefabName);

                    if (item != null)
                        if (item.ItemId == objective.ItemId)
                            meetsRequirement = item.InventoryCategoryID is not ItemFilterCategory.None
                                                                        and not ItemFilterCategory.QuestItems
                                               || objective.LevelId == player.Character.LevelData.LevelId;
                }

                if (objective.MultiScorePrefabs != null)
                    if (objective.MultiScorePrefabs.Count > 0 && !meetsRequirement)
                        if (objective.MultiScorePrefabs.Contains(prefabName))
                            if (objective.LevelId > 0 && objective.LevelId == player.Room.LevelInfo.LevelId)
                                meetsRequirement = true;

                if (!meetsRequirement && objective.LevelId == player.Character.LevelData.LevelId && type == ObjectiveEnum.MinigameMedal)
                    meetsRequirement = true;

                if (!meetsRequirement)
                    continue;

                if (setObjective)
                    objective.CountLeft = objective.Total - count;
                else
                    objective.CountLeft -= count;

                if (objective.ObjectiveType is ObjectiveEnum.AlterandReceiveitem or ObjectiveEnum.Receiveitem or ObjectiveEnum.Giveitem)
                {
                    objective.CountLeft = 0;
                }
                else if (objective.ObjectiveType is ObjectiveEnum.Deliver)
                {
                    var item = itemCatalog.GetItemFromId(objective.ItemId);

                    objective.CountLeft = player.Character.Data.Inventory.Items.TryGetValue(objective.ItemId, out var itemModel) && item != null
                        ? objective.Total - itemModel.Count
                        : 0;
                }

                if (objective.CountLeft <= 0)
                {
                    objective.CountLeft = 0;
                    objective.Completed = true;

                    var leftObjectives = quest.Objectives.Where(x => x.Value.Completed != true);

                    if (leftObjectives.Any())
                        quest.CurrentOrder = leftObjectives.Min(x => x.Value.Order);

                    player.SendXt("no", quest.Id, objectiveKVP.Key);

                    hasObjComplete = true;

                    switch (objective.ObjectiveType)
                    {
                        case ObjectiveEnum.Talkto:
                            foreach (var obj in quest.Objectives.Values
                                .Where(obj => obj.Order == objective.Order &&
                                    obj.ObjectiveType == ObjectiveEnum.Deliver &&
                                    obj.ItemId > 0
                                )
                            )
                            {
                                var item = itemCatalog.GetItemFromId(obj.ItemId);

                                if (player.Character.Data.Inventory.Items.TryGetValue(obj.ItemId, out var itemModel) && item != null)
                                    player.CheckObjective(ObjectiveEnum.Deliver, gameObjectId, item.PrefabName, itemModel.Count, questCatalog, itemCatalog);
                            }
                            break;
                        case ObjectiveEnum.Deliver:
                            var deliveredItem = itemCatalog.GetItemFromId(objective.ItemId);

                            if (deliveredItem != null)
                            {
                                player.RemoveItem(deliveredItem, objective.Total, itemCatalog);
                                player.SendUpdatedInventory();
                            }
                            break;
                        case ObjectiveEnum.Receiveitem:
                        case ObjectiveEnum.Giveitem:
                        case ObjectiveEnum.AlterandReceiveitem:
                            var givenItem = itemCatalog.GetItemFromId(objective.ItemId);

                            if (givenItem != null)
                            {
                                player.AddItem(givenItem, objective.Total, itemCatalog);
                                player.SendUpdatedInventory();
                            }
                            break;
                    }
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
                player.UpdateNpcsInLevel(quest, questCatalog);
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
