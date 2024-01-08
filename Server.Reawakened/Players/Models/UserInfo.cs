using Server.Base.Core.Models;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Services;
using Server.Reawakened.Players.Enums;
using Server.Reawakened.Players.Models.System;

namespace Server.Reawakened.Players.Models;

public class UserInfo : PersistantData
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

    public UserInfo()
    {
        CharacterIds = [];
        Mail = [];
    }

    public UserInfo(int userId, Gender gender, DateTime dateOfBirth, string region,
        string signUpExperience, RandomKeyGenerator kGen, ServerRConfig config)
    {
        Region = region;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        SignUpExperience = signUpExperience;

        LastCharacterSelected = string.Empty;
        AuthToken = kGen.GetRandomKey<UserInfo>(userId.ToString());

        Member = config.DefaultMemberStatus;
        TrackingShortId = config.DefaultTrackingShortId;
        ChatLevel = config.DefaultChatLevel;

        CharacterIds = [];
        Mail = [];
    }
}
