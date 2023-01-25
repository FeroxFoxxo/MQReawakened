using A2m.Server;
using Server.Reawakened.Characters.Models;
using Server.Reawakened.Core.Models;
using Server.Reawakened.Core.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Services;
using System.Globalization;

namespace Protocols.External._c__CharacterInfoHandler;

public class CreateCharacter : ExternalProtocol
{
    public override string ProtocolName => "cr";

    public UserInfoHandler UserInfoHandler { get; set; }
    public NameGenSyllables NameGenSyllables { get; set; }
    public ServerConfig ServerConfig { get; set; }

    public override void Run(string[] message)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        var firstName = message[5];
        var middleName = message[6];
        var lastName = message[7];
        var gender = int.Parse(message[8]);
        var characterData = new CharacterDetailedModel(message[9]);
        var tribe = (TribeType)int.Parse(message[10]);

        var names = new [] { firstName, middleName, lastName };

        var userInfo = NetState.Get<Player>().UserInfo;

        characterData.Allegiance = tribe;
        characterData.CharacterName = string.Join(' ', names);
        characterData.Gender = gender;
        characterData.UserUuid = userInfo.UserId.ToString();

        if (NameGenSyllables.IsNameReserved(names, UserInfoHandler))
        {
            var suggestion = NameGenSyllables.GetRandomName(gender, UserInfoHandler);
            SendXt("cr", "0", suggestion[0], suggestion[1], suggestion[2]);
        }
        else if (userInfo.Characters.Count > ServerConfig.MaxCharacterCount)
        {
            SendXt("cr", "1");
        }
        else
        {
            SendXt("cr", userInfo.UserId.ToString(), characterData.ToString(), "0", "0");
        }
    }
}
