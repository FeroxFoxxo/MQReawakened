using Server.Base.Core.Models;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Services;
using Server.Reawakened.Players.Enums;
using Server.Reawakened.Players.Models.System;

namespace Server.Reawakened.Database.Users;
public class UserInfoDbEntry : PersistantData
{
    public List<int> CharacterIds { get; set; }
    public Dictionary<int, SystemMailModel> Mail { get; set; }

    public string LastCharacterSelected { get; set; }

    public string AuthToken { get; set; }

    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public bool Member { get; set; }
    public string SignUpExperience { get; set; }
    public string Region { get; set; }
    public string TrackingShortId { get; set; }
    public int ChatLevel { get; set; }

    public UserInfoDbEntry() => InitializeList();

    public UserInfoDbEntry(int userId, Gender gender, DateTime dateOfBirth, string region,
        string signUpExperience, RandomKeyGenerator kGen, ServerRConfig config)
    {
        Region = region;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        SignUpExperience = signUpExperience;

        LastCharacterSelected = string.Empty;
        AuthToken = kGen.GetRandomKey<UserInfoModel>(userId.ToString());

        Member = config.DefaultMemberStatus;
        TrackingShortId = config.DefaultTrackingShortId;
        ChatLevel = config.DefaultChatLevel;

        InitializeList();
    }

    public void InitializeList()
    {
        CharacterIds = [];
        Mail = [];
    }
}
