using Achievement.Types;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;

namespace Web.Apps.Achievements.Services;

public class AchievementHandler(EventSink sink) : IService
{
    public AchievementStaticJson.AchievementDefinition Definitions { get; private set; }

    public void Initialize() => sink.WorldLoad += LoadAchievements;

    private void LoadAchievements() =>
        Definitions = new AchievementStaticJson.AchievementDefinition
        {
            status = true,
            achievements = [],
            categories = [],
            characterTitles = [],
            types = new AchievementType
            {
                conditions = [],
                rewards = [],
                timeWindows = []
            }
        };
}
