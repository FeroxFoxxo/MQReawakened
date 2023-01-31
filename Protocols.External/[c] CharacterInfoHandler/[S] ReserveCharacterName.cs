using Server.Reawakened.Characters.Helpers;
using Server.Reawakened.Core.Network.Protocols;
using Server.Reawakened.Players.Enums;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Services;

namespace Protocols.External._c__CharacterInfoHandler;

public class ReserveCharacterName : ExternalProtocol
{
    public const int Count = 4;

    public override string ProtocolName => "cS";

    public NameGenSyllables NameGenSyllables { get; set; }
    public UserInfoHandler UserInfoHandler { get; set; }

    public override void Run(string[] message)
    {
        var gender = int.Parse(message[5]);
        var name = new[] { message[6], message[7], message[8] };

        if (NameGenSyllables.IsNameReserved(name, UserInfoHandler))
            SendXt("cT", 0, GetNames(gender));
        else if (!NameGenSyllables.IsPossible(gender, name))
            SendXt("cT", 1);
        else
            SendXt("cS", string.Empty);
    }

    private string GetNames(int gender)
    {
        var sb = new SeparatedStringBuilder('%');

        for (var i = 0; i < Count; i++)
            sb.Append(NameGenSyllables.GetRandomName(gender, UserInfoHandler));

        return sb.ToString();
    }
}
