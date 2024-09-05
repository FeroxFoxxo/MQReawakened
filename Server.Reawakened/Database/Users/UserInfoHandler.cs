using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Base.Database.Abstractions;
using Server.Base.Database.Accounts;
using Server.Base.Network;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Network.Services;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Enums;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Services;
using System.Globalization;
using System.Net;

namespace Server.Reawakened.Database.Users;

public class UserInfoHandler(WorldHandler worldHandler, RandomKeyGenerator randomKeyGenerator, ServerRConfig config,
    PlayerContainer playerContainer, CharacterHandler characterHandler, ReawakenedLock dbLock,
    IServiceProvider services) : DataHandler<UserInfoDbEntry, ReawakenedDatabase, ReawakenedLock>(services, dbLock)
{
    public override bool HasDefault => true;

    public void InitializeUser(NetState state)
    {
        var account = state.Get<AccountModel>() ?? throw new NullReferenceException("Account not found!");
        var userInfo = GetUserFromId(account.Id) ?? throw new NullReferenceException("User info not found!");

        state.Set(userInfo);
        state.Set(new Player(account, userInfo, state, worldHandler, playerContainer, characterHandler));
    }

    public override UserInfoDbEntry CreateDefault()
    {
        Gender gender;

        while (true)
        {
            Logger.LogInformation("Gender: ");

            if (Enum.TryParse(ConsoleExt.ReadOrEnv("DEFAULT_GENDER", Logger), true, out gender))
                break;

            Logger.LogWarning("Incorrect input! Must be either: {Types}",
                string.Join(", ", Enum.GetNames<Gender>()));
        }

        DateTime dob;

        while (true)
        {
            Logger.LogInformation("Date Of Birth: ");

            if (DateTime.TryParse(ConsoleExt.ReadOrEnv("DEFAULT_DOB", Logger), out dob))
                break;

            Logger.LogWarning("Incorrect input! Must be a date!");
        }

        return new UserInfoDbEntry(CreateNewId(), gender, dob, RegionInfo.CurrentRegion.Name, config.DefaultSignUpExperience, randomKeyGenerator, config);
    }

    public UserInfoDbEntry Create(IPAddress ip, int id, Gender gender, DateTime dob, string region, string signUpExperience)
    {
        Logger.LogInformation("Login: {Address}: Creating new user info '{Id}' of gender '{Gender}', DOB '{DOB}', region '{region}' and sign up experience '{SignUpExperience}'.",
            ip, id, gender, dob, region, signUpExperience);

        var user = new UserInfoDbEntry(id, gender, dob, region, signUpExperience, randomKeyGenerator, config);

        Add(user, id);

        return user;
    }

    public UserInfoModel GetUserFromId(int id)
    {
        var userInfoEntry = Get(id);

        if (userInfoEntry == null)
            return null;

        var userInfo = new UserInfoModel(userInfoEntry);

        foreach (var characterId in userInfo.CharacterIds.ToList())
        {
            var character = characterHandler.GetCharacterFromId(characterId);

            if (character == null)
            {
                characterHandler.DeleteCharacter(characterId, userInfo);
                continue;
            }

            if (character.UserUuid != userInfo.Id)
            {
                userInfo.CharacterIds.Remove(characterId);
                continue;
            }
        }

        if (!string.IsNullOrEmpty(userInfo.LastCharacterSelected))
            if (userInfo.CharacterIds.Count == 0)
                userInfo.Write.LastCharacterSelected = string.Empty;

        return userInfo;
    }
}
