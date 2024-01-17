using Achievement.CharacterData;
using Achievement.StaticData;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.XMLs.BundlesInternal;
using Server.Reawakened.XMLs.Enums;

namespace Server.Reawakened.Players.Extensions;

public static class PlayerAchievementExtensions
{
    public static void CheckAchievement(this Player player, AchConditionType achType, string achValue, int count = 1)
    {
        var posCond = player.DatabaseContainer.InternalAchievement.PossibleConditions;
        var type = (int)achType;

        if (!posCond.TryGetValue(type, out var value))
            return;

        if (!value.Contains(achValue))
            return;

        var pAchObj = player.Character.AchievementObjectives;

        if (!pAchObj.ContainsKey(type))
            pAchObj.Add(type, []);

        pAchObj[type].TryAdd(achValue, 0);

        var achievements = player.DatabaseContainer.InternalAchievement.Definitions.achievements
            .Where(a => a.conditions.Any(c => c.typeId == type && c.description == achValue));

        if (!achievements.Any())
            return;

        var cond = achievements.Select(
            a => new KeyValuePair<AchievementStaticData, List<AchievementDefinitionConditions>>(
                a,
                a.conditions.Where(c => c.typeId == type && c.description == achValue).ToList()
            )
        );

        var inProgCond = cond.Where(a => a.Value.Any(c => c.GetObjectiveLeft(pAchObj[type][achValue]) > 1));

        if (!inProgCond.Any())
            return;

        pAchObj[type][achValue] += count;

        var oInProg = inProgCond.OrderBy(a => a.Value.Min(c => c.GetObjectiveLeft(pAchObj[type][achValue])));

        var firstAch = oInProg.FirstOrDefault();

        if (firstAch.Key == null)
            return;

        var ach = player.Character.GetAchievement(firstAch.Key);

        var oCond = firstAch.Value.OrderBy(c => pAchObj[type][achValue] - c.goal);
        var firstCond = oCond.First();

        player.SendXt("Ap",
            firstAch.Key.id,
            firstCond.id,
            firstCond.GetObjectiveLeft(pAchObj[type][achValue]),
            ach.GetTotalProgress()
        );
    }

    public static int GetObjectiveLeft(this AchievementDefinitionConditions cond, int count) => count - cond.goal;

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

            // UNUSED
            ctime = long.MinValue,
            mtime = long.MinValue,
        };

    public static List<CharacterAchievement> GetAllAchievements(this CharacterModel character, InternalAchievement internalAchievement)
    {
        var _characterId = character.Id;
        var obs = character.AchievementObjectives;

        return internalAchievement.Definitions.achievements
            .Select(x => GetAchievement(character, x)).ToList();
    }
}
