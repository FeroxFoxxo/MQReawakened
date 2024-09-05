using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Models;
using Server.Base.Network;
using Server.Reawakened.Players.Enums;
using Server.Reawakened.Players.Models.System;

namespace Server.Reawakened.Database.Users;

public class UserInfoModel(UserInfoDbEntry entry) : INetStateData
{
    public int Id => entry.Id;
    public UserInfoDbEntry Write => entry;

    public List<int> CharacterIds => entry.CharacterIds;
    public Dictionary<int, SystemMailModel> Mail => entry.Mail;
    public string LastCharacterSelected => entry.LastCharacterSelected;
    public string AuthToken => entry.AuthToken;
    public Gender Gender => entry.Gender;
    public DateTime DateOfBirth => entry.DateOfBirth;
    public bool Member => entry.Member;
    public string SignUpExperience => entry.SignUpExperience;
    public string Region => entry.Region;
    public string TrackingShortId => entry.TrackingShortId;
    public int ChatLevel => entry.ChatLevel;

    public void RemovedState(NetState _, IServiceProvider services, Microsoft.Extensions.Logging.ILogger logger)
    {
        logger.LogTrace("Saving user info data for id {Id}", Id);

        var handler = services.GetRequiredService<UserInfoHandler>();
        handler.Update(Write);
    }
}
