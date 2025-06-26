using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Database.Users;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Achievements;
using static A2m.Server.QuestStatus;

namespace Server.Reawakened.Players.Extensions;

public static class PlayerExtensions
{
    public static void AddGear(this Player currentPlayer, string gear, ItemCatalog itemCatalog)
    {
        var item = itemCatalog.GetItemFromPrefabName(gear);

        if (item != null)
        {
            if (!currentPlayer.Character.Inventory.Items.ContainsKey(item.ItemId))
            {
                currentPlayer.AddItem(item, 1, itemCatalog);
                currentPlayer.SendUpdatedInventory();
            }
        }
    }

    public static void TeleportPlayer(this Player player, float x, float y, bool isBackPlane)
    {
        var coordinates = new PhysicTeleport_SyncEvent(player.GameObjectId.ToString(),
            player.Room.Time, x, y, isBackPlane);

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

        reputation *= (int)(1 + player.Character.StatusEffects.GetEffect(ItemEffectType.ExperienceMultiplier) * 0.01);
        reputation += player.Character.Reputation;

        while (reputation > player.Character.ReputationForNextLevel)
        {
            var newLevel = player.Character.GlobalLevel + 1;

            player.Character.SetLevelXp(newLevel, config);
            player.SendLevelUp();
        }

        player.Character.Write.Reputation = reputation;

        player.SendXt("cp", player.Character.Reputation, player.Character.ReputationForNextLevel);
    }

    public static void TradeWithPlayer(this Player origin, ItemCatalog itemCatalog, ItemRConfig config)
    {
        var tradeModel = origin.TempData.TradeModel;

        if (tradeModel == null)
            return;

        var tradingPlayer = tradeModel.TradingPlayer;

        foreach (var item in tradeModel.ItemsInTrade)
        {
            var itemDesc = itemCatalog.GetItemFromId(item.Key);

            tradeModel.TradingPlayer.AddItem(itemDesc, item.Value, itemCatalog);
            origin.RemoveItem(itemDesc, item.Value, itemCatalog, config);
        }

        tradingPlayer.Character.Write.Cash += tradeModel.BananasInTrade;
        origin.Character.Write.Cash -= tradeModel.BananasInTrade;
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

    public static void AddBananas(this Player player, float collectedBananas, InternalAchievement internalAchievement, Microsoft.Extensions.Logging.ILogger logger)
    {
        collectedBananas *= (float)(1 + player.Character.StatusEffects.GetEffect(ItemEffectType.BananaMultiplier) * 0.01);

        player.Character.Write.Cash += collectedBananas;
        player.SendCashUpdate();

        player.CheckAchievement(AchConditionType.CollectBanana, [], internalAchievement, logger, (int)Math.Floor(collectedBananas));
    }

    public static void RemoveBananas(this Player player, int collectedBananas)
    {
        player.Character.Write.Cash -= collectedBananas;

        if (player.Character.Cash < 0)
            player.Character.Write.Cash = 0;

        player.SendCashUpdate();
    }

    public static void AddNCash(this Player player, int collectedNCash)
    {
        player.Character.Write.NCash += collectedNCash;
        player.SendCashUpdate();
    }

    public static void RemoveNCash(this Player player, int collectedNCash)
    {
        player.Character.Write.NCash -= collectedNCash;

        if (player.Character.Write.NCash < 0)
            player.Character.Write.NCash = 0;

        player.SendCashUpdate();
    }

    public static void AddPoints(this Player player)
    {
        player.Character.Write.BadgePoints += 100;
        player.SendLevelUp();
    }

    public static void SendCashUpdate(this Player player) =>
        player.SendXt("ca", Math.Floor(player.Character.Cash), Math.Floor(player.Character.NCash));

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

        // Allows early 2012 to load
        if (worldHandler.Config.GameVersion >= GameVersion.vMinigames2012)
            player.SendXt("lw", error, levelName, surroundingLevels);
        else
            player.SendXt("lw", error, levelName, string.Empty, surroundingLevels);
    }

    public static void SetCharacterSelected(this Player player, CharacterModel character)
    {
        player.Character = character;
        player.NetState.Set(character);
        player.UserInfo.Write.LastCharacterSelected = player.CharacterName;
    }

    public static void AddCharacter(this Player player, CharacterModel character) =>
        player.UserInfo.CharacterIds.Add(character.Id);

    public static void DeleteCharacter(this CharacterHandler characterHandler, int id, UserInfoModel userInfo)
    {
        userInfo.CharacterIds.Remove(id);

        characterHandler.Remove(id);

        if (userInfo.CharacterIds.Count > 0)
        {
            var character = characterHandler.GetCharacterFromId(userInfo.CharacterIds.First());

            if (character != null)
            {
                userInfo.Write.LastCharacterSelected = character.CharacterName;
                return;
            }
        }

        userInfo.Write.LastCharacterSelected = string.Empty;
    }

    public static void LevelUp(this Player player, int level, WorldStatistics worldStatistics,
    ServerRConfig config, Microsoft.Extensions.Logging.ILogger logger)
    {
        player.Character.SetLevelXp(level, config);
        player.SendLevelUp();

        if (player.Character.Pets.TryGetValue(player.GetEquippedPetId(config), out var pet))
            pet.GainEnergy(player, player.GetMaxPetEnergy(worldStatistics, config));

        //Temporary NCash reward until original level up system is implemented.
        player.AddNCash(config.LevelUpNCashReward);
        player.SendCashUpdate();
        //(Needed for gameplay improvements as NC is currently unobtainable)

        logger.LogTrace("{Name} leveled up to {Level}", player.CharacterName, level);
    }

    public static void DiscoverTribe(this Player player, TribeType tribe)
    {
        if (player.Character.HasAddedDiscoveredTribe(tribe))
        {
            // Set tribe on 2011-2013
            if (player.Character.Allegiance == TribeType.Invalid
                && tribe is TribeType.Shadow
                or TribeType.Outlaw or TribeType.Bone
                or TribeType.Wild or TribeType.Grease)
                player.Character.Write.Allegiance = tribe;

            player.SendXt("cB", (int)tribe);
        }
    }

    public static void DiscoverAllTribes(this Player player)
    {
        foreach (TribeType tribe in Enum.GetValues(typeof(TribeType)))
            player.DiscoverTribe(tribe);
    }

    public static void AddSlots(this Player player, bool hasPet, ItemRConfig config)
    {
        var hotbarButtons = player.Character.Hotbar.HotbarButtons;

        for (var i = 0; i < (hasPet ? 5 : 4); i++)
        {
            if (!hotbarButtons.ContainsKey(i))
            {
                var itemModel = config.EmptySlot;
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

        player.Room.Logger.LogDebug("Checking {Type} objective for '{Prefab}' ({Id}) of count {Count}.", type, prefabName, gameObjectId, count);

        foreach (var quest in player.Character.QuestLog)
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
                        objective.LevelId == player.Character.LevelId)
                        meetsRequirement = true;

                if (objective.ItemId > 0 && !meetsRequirement)
                {
                    var item = itemCatalog.GetItemFromPrefabName(prefabName);

                    if (item != null)
                        if (item.ItemId == objective.ItemId)
                            meetsRequirement = item.InventoryCategoryID is not ItemFilterCategory.None
                                                                        and not ItemFilterCategory.QuestItems
                                               || objective.LevelId == player.Character.LevelId;
                }

                if (objective.MultiScorePrefabs != null)
                    if (objective.MultiScorePrefabs.Count > 0 && !meetsRequirement)
                        if (objective.MultiScorePrefabs.Contains(prefabName))
                            if (objective.LevelId > 0 && objective.LevelId == player.Room.LevelInfo.LevelId)
                                meetsRequirement = true;

                if (!meetsRequirement && objective.LevelId == player.Character.LevelId && type == ObjectiveEnum.MinigameMedal)
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

                    objective.CountLeft = player.Character.Inventory.Items.TryGetValue(objective.ItemId, out var itemModel) && item != null
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

                                if (player.Character.Inventory.Items.TryGetValue(obj.ItemId, out var itemModel) && item != null)
                                    player.CheckObjective(ObjectiveEnum.Deliver, gameObjectId, item.PrefabName, itemModel.Count, questCatalog, itemCatalog);
                            }
                            break;
                        case ObjectiveEnum.Deliver:
                            var deliveredItem = itemCatalog.GetItemFromId(objective.ItemId);

                            if (deliveredItem != null)
                            {
                                player.RemoveItem(deliveredItem, objective.Total, itemCatalog, itemCatalog.ItemConfig);
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
            from player in sentPlayer.Room.GetPlayers()
            select player
        )
            player.SendXt("iq", sentPlayer.UserId, sentPlayer.Character.Equipment);
    }
}
