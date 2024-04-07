using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Models;
using Server.Base.Core.Configs;
using Server.Base.Core.Events;
using Server.Base.Core.Services;
using Server.Base.Network;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Services;
using Server.Reawakened.Players.Enums;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Rooms.Services;
using System.Globalization;
using System.Net;

namespace Server.Reawakened.Players.Services;

public class UserInfoHandler(EventSink sink, ILogger<UserInfo> logger, WorldHandler worldHandler,
    RandomKeyGenerator randomKeyGenerator, ServerRConfig config, InternalRConfig rConfig,
    InternalRwConfig rwConfig, PlayerContainer playerContainer, CharacterHandler characterHandler) : DataHandler<UserInfo>(sink, logger, rConfig, rwConfig)
{
    public override bool HasDefault => true;

    public void InitializeUser(NetState state)
    {
        var account = state.Get<Account>();

        var userId = account?.Id ?? throw new NullReferenceException("Account not found!");

        if (!GetInternal().TryGetValue(userId, out var value))
            throw new NullReferenceException();

        state.Set(new Player(account, value, state, worldHandler, playerContainer, characterHandler));
    }

    public override UserInfo CreateDefault()
    {
        Gender gender;

        while (true)
        {
            Logger.LogInformation("Gender: ");

            if (Enum.TryParse(Console.ReadLine(), true, out gender))
                break;

            Logger.LogWarning("Incorrect input! Must be either: {Types}",
                string.Join(", ", Enum.GetNames<Gender>()));
        }

        DateTime dob;

        while (true)
        {
            Logger.LogInformation("Date Of Birth: ");

            if (DateTime.TryParse(Console.ReadLine(), out dob))
                break;

            Logger.LogWarning("Incorrect input! Must be a date!");
        }

        return new UserInfo(CreateNewId(), gender, dob, RegionInfo.CurrentRegion.Name, config.DefaultSignUpExperience, randomKeyGenerator, config);
    }

    public UserInfo Create(IPAddress ip, int id, Gender gender, DateTime dob, string region, string signUpExperience)
    {
        Logger.LogInformation("Login: {Address}: Creating new user info '{Id}' of gender '{Gender}', DOB '{DOB}', region '{region}' and sign up experience '{SignUpExperience}'.",
            ip, id, gender, dob, region, signUpExperience);

        var user = new UserInfo(id, gender, dob, region, signUpExperience, randomKeyGenerator, config);

        Add(user, id);

        return user;
    }
}
