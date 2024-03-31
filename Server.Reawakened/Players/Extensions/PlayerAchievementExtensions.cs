using Achievement.CharacterData;
using Achievement.StaticData;
using Microsoft.Extensions.DependencyInjection;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Extensions;

namespace Server.Reawakened.Players.Extensions;

public static class PlayerAchievementExtensions
{
    public static void CheckAchievement(this Player player, AchConditionType achType, string achValue,
        InternalAchievement internalAchievement, Microsoft.Extensions.Logging.ILogger logger, int count = 1)
    {
        if (string.IsNullOrEmpty(achValue))
            achValue = "any";

        if (player == null)
            return;

        var posCond = internalAchievement.PossibleConditions;
        var type = (int)achType;
        achValue = achValue.ToLower();

        if (!posCond.TryGetValue(type, out var value))
            return;

        if (!value.Contains(achValue))
            return;

        var pAchObj = player.Character.AchievementObjectives;

        if (!pAchObj.ContainsKey(type))
            pAchObj.Add(type, []);

        pAchObj[type].TryAdd(achValue, 0);

        var achievements = internalAchievement.Definitions.achievements
            .Where(a => a.conditions.Any(c => c.typeId == type && c.description == achValue))
            .ToList();

        if (achievements.Count == 0)
            return;

        var cond = achievements.Select(
            a => new KeyValuePair<AchievementStaticData, List<AchievementDefinitionConditions>>(
                a,
                a.conditions.Where(c => c.typeId == type && c.description == achValue).ToList()
            )
        ).ToList();

        var inProgCond = cond.Where(a => a.Value.Any(c => c.GetObjectiveLeft(pAchObj[type][achValue]) > 0)).ToList();

        if (inProgCond.Count == 0)
            return;

        pAchObj[type][achValue] += count;

        var oInProg = inProgCond.OrderBy(a =>
            a.Key.repeatable ?
                int.MaxValue :
                player.Character.GetAchievement(a.Key).GetAmountLeft()
            ).ToList();

        var firstAchievement = true;

        foreach (var achievement in oInProg)
        {
            var currentAchievement = player.Character.GetAchievement(achievement.Key);
            var amountLeft = currentAchievement.GetAmountLeft();

            if (!player.TempData.CurrentAchievements.ContainsKey(type))
                player.TempData.CurrentAchievements.Add(type, []);

            var containsAch = player.TempData.CurrentAchievements[type].Contains(achValue);

            if (amountLeft <= 0 || firstAchievement && !containsAch)
            {
                player.SendXt("Ap",
                    -1,
                    currentAchievement.id,
                    -1,
                    currentAchievement.GetAmountLeft(),
                    currentAchievement.GetTotalProgress()
                );

                firstAchievement = false;
            }

            if (!containsAch)
                player.TempData.CurrentAchievements[type].Add(achValue);

            var itemCatalog = internalAchievement.Services.GetRequiredService<ItemCatalog>();

            if (amountLeft <= 0)
                achievement.Key.rewards.RewardPlayer(player, itemCatalog, internalAchievement, logger);
        }
    }

    public static int GetObjectiveLeft(this AchievementDefinitionConditions cond, int count) => cond.goal - count;

    public static int GetAmountLeft(this CharacterAchievement achievement) => achievement.goal - GetTotalProgress(achievement);

    public static int GetTotalProgress(this CharacterAchievement achievement)
    {
        var count = 0;

        foreach (var c in achievement.conditions)
            count += c.value;

        return count;
    }

    public static CharacterAchievement GetAchievement(this CharacterModel character, AchievementStaticData achievement) =>
        new()
        {
            characterId = character.Id,
            id = achievement.id,
            conditions = achievement.conditions.Select(c =>
            {
                var value = 0;

                if (character.AchievementObjectives.TryGetValue(c.typeId, out var obj))
                    if (obj.TryGetValue(c.description, out var count))
                        value = count > c.goal ? c.goal : count;

                return new CharacterCondition()
                {
                    id = c.id,
                    characterId = character.Id,

                    // GOAL TO COMPLETE
                    completionCount = c.goal,

                    // CURRENT PROGRESS
                    value = value,

                    // UNUSED
                    ctime = long.MinValue, // COMPLETION TIME
                    mtime = long.MinValue, // MODIFIED TIME
                };
            }).ToList(),
            goal = achievement.goal,

            // UNUSED
            ctime = long.MinValue,
            mtime = long.MinValue,
        };

    public static List<CharacterAchievement> GetAllAchievements(this CharacterModel character, InternalAchievement internalAchievement)
    {
        var _characterId = character.Id;

        return internalAchievement.Definitions.achievements
            .Select(x => GetAchievement(character, x)).ToList();
    }
}
