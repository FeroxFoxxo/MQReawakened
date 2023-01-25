using A2m.Server;
using Server.Base.Accounts.Modals;
using Server.Reawakened.Core.Models;
using Server.Reawakened.Core.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Modals;
using Server.Reawakened.Players.Services;

namespace Protocols.External._c__CharacterInfoHandler;

public class CreateCharacter : ExternalProtocol
{
    public override string ProtocolName => "cr";

    public UserInfoHandler UserInfoHandler { get; set; }
    public NameGenSyllables NameGenSyllables { get; set; }
    public ServerConfig ServerConfig { get; set; }

    public override void Run(string[] message)
    {
        var firstName = message[5];
        var middleName = message[6];
        var lastName = message[7];
        var gender = int.Parse(message[8]);
        var characterData = message[9];
        var tribe = (TribeType)int.Parse(message[10]);

        var names = new string[3] { firstName, middleName, lastName };

        if (NameGenSyllables.IsNameReserved(names, UserInfoHandler))
        {
            var suggestion = NameGenSyllables.GetRandomName(gender, UserInfoHandler);
            SendXt("cr", "0", suggestion[0], suggestion[1], suggestion[2]);
        }
        else if (NetState.Get<Player>().UserInfo.Characters.Count > ServerConfig.MaxCharacterCount)
        {
            SendXt("cr", "1");
        }
        else
        {
            Console.WriteLine("MAKE PLAYER");
        }
    }
}
