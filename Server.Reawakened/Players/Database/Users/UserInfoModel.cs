using Server.Reawakened.Players.Enums;
using Server.Reawakened.Players.Models.System;

namespace Server.Reawakened.Players.Database.Users;

public class UserInfoModel(UserInfoDbEntry entry)
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
}
