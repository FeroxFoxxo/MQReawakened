using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Models;
using Server.Base.Network;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Models.Misc;
using Server.Reawakened.Players.Models.Pets;
using Server.Reawakened.Players.Models.System;

namespace Server.Reawakened.Database.Characters;

public class CharacterModel(CharacterDbEntry entry, GameVersion version) : CharacterDataModel(entry, version), INetStateData
{
    public int LevelId => Write.LevelId;
    public string SpawnPointId => Write.SpawnPointId;
    public Dictionary<int, List<int>> CollectedIdols => Write.CollectedIdols;
    public List<EmailHeaderModel> Emails => Write.Emails;
    public Dictionary<int, EmailMessageModel> EmailMessages => Write.EmailMessages;
    public List<int> Events => Write.Events;
    public Dictionary<int, Dictionary<string, int>> AchievementObjectives => Write.AchievementObjectives;
    public Dictionary<string, float> BestMinigameTimes => Write.BestMinigameTimes;
    public Dictionary<string, DailiesModel> CurrentCollectedDailies => Write.CurrentCollectedDailies;
    public Dictionary<string, DailiesModel> CurrentQuestDailies => Write.CurrentQuestDailies;
    public Dictionary<string, PetModel> Pets => Write.Pets;

    public void RemovedState(NetState _, IServiceProvider services, Microsoft.Extensions.Logging.ILogger logger)
    {
        logger.LogTrace("Saving character data for {Name}", CharacterName);

        var handler = services.GetRequiredService<CharacterHandler>();
        handler.Update(Write);
    }
}
