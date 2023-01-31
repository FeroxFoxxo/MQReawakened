using Server.Base.Core.Models;
using Server.Reawakened.Core.Network.Services;
using Server.Reawakened.Players.Enums;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Models.System;

namespace Server.Reawakened.Players.Models;

public class UserInfo : PersistantData
{
    public Dictionary<int, CharacterDataModel> Characters { get; set; }
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
    }

    public UserInfo(int userId, Gender gender, DateTime dateOfBirth, string region, RandomKeyGenerator kGen)
    {
        Region = region;
        UserId = userId;
        Gender = gender;
        DateOfBirth = dateOfBirth;

        LastCharacterSelected = string.Empty;
        SignUpExperience = "unknown";
        Member = true;
        TrackingShortId = "false";
        AuthToken = kGen.GetRandomKey<UserInfo>(userId.ToString());
        ChatLevel = 3;
        
        Characters = new Dictionary<int, CharacterDataModel>();
        Mail = new Dictionary<int, SystemMailModel>();
    }
}
