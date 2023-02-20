using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Models;
using Server.Base.Core.Events;
using Server.Base.Core.Services;
using Server.Base.Network;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Services;
using Server.Reawakened.Players.Enums;
using Server.Reawakened.Players.Models;
using System.Globalization;

namespace Server.Reawakened.Players.Services;

public class UserInfoHandler : DataHandler<UserInfo>
{
    private readonly ServerStaticConfig _config;
    private readonly RandomKeyGenerator _randomKeyGenerator;

    public UserInfoHandler(EventSink sink, ILogger<UserInfo> logger,
        RandomKeyGenerator randomKeyGenerator, ServerStaticConfig config) : base(sink, logger)
    {
        _randomKeyGenerator = randomKeyGenerator;
        _config = config;
    }

    public void InitializeUser(NetState state)
    {
        var userId = state.Get<Account>()?.UserId ?? throw new ArgumentNullException(nameof(Account));

        if (!Data.ContainsKey(userId))
            throw new NullReferenceException();

        state.Set(new Player(Data[userId]));
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

        return new UserInfo(Data.Count, gender, dob, RegionInfo.CurrentRegion.Name, _randomKeyGenerator, _config);
    }

    public override UserInfo Create(NetState netState, params string[] obj) => throw new NotImplementedException();
}
