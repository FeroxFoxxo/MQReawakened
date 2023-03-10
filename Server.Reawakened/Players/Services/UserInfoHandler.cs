using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Models;
using Server.Base.Core.Events;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
using Server.Base.Network;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Services;
using Server.Reawakened.Players.Enums;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models;
using System.Globalization;
using System.Net;

namespace Server.Reawakened.Players.Services;

public class UserInfoHandler : DataHandler<UserInfo>
{
    private readonly ServerRConfig _config;
    private readonly RandomKeyGenerator _randomKeyGenerator;
    private readonly PlayerHandler _playerHandler;

    public UserInfoHandler(EventSink sink, ILogger<UserInfo> logger,
        RandomKeyGenerator randomKeyGenerator, ServerRConfig config, InternalRConfig internalConfig,
        PlayerHandler playerHandler) :
        base(sink, logger, internalConfig)
    {
        _randomKeyGenerator = randomKeyGenerator;
        _config = config;
        _playerHandler = playerHandler;

        _playerHandler.UserInfoHandler = this;
    }

    public void InitializeUser(NetState state)
    {
        var userId = state.Get<Account>()?.UserId ?? throw new NullReferenceException("Account not found!");

        if (!Data.ContainsKey(userId))
            throw new NullReferenceException();

        state.Set(new Player(Data[userId], state, _playerHandler));
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

        return new UserInfo(Data.Count, gender, dob, RegionInfo.CurrentRegion.Name, _config.DefaultSignUpExperience, _randomKeyGenerator, _config);
    }

    public UserInfo Create(IPAddress ip, int id, Gender gender, DateTime dob, string region, string signUpExperience)
    {
        Logger.LogInformation("Login: {Address}: Creating new user info '{Id}' of gender '{Gender}', DOB '{DOB}', region '{region}' and sign up experience '{SignUpExperience}'.",
            ip, id, gender, dob, region, signUpExperience);

        var user = new UserInfo(id, gender, dob, region, signUpExperience, _randomKeyGenerator, _config);
        Data.Add(Data.Count, user);
        return user;
    }
}
