using Server.Reawakened.Configs;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Enums;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Services;

namespace Protocols.External._c__CharacterInfoHandler;

public class ReserveCharacterName : ExternalProtocol
{
    public override string ProtocolName => "cS";

    public NameGenSyllables NameGenSyllables { get; set; }
    public CharacterHandler CharacterHandler { get; set; }
    public ServerRConfig ServerConfig { get; set; }

    public override void Run(string[] message)
    {
        var gender = (Gender)int.Parse(message[5]);
        var name = new[] { message[6], message[7], message[8] };

        if (NameGenSyllables.IsNameReserved(name, CharacterHandler))
            SendXt("cT", 0, GetNames(gender));
        else if (!NameGenSyllables.IsPossible(gender, name))
            SendXt("cT", 1);
        else
            SendXt("cS", string.Empty);
    }

    private string GetNames(Gender gender)
    {
        var sb = new SeparatedStringBuilder('%');

        for (var i = 0; i < ServerConfig.ReservedNameCount; i++)
            sb.Append(NameGenSyllables.GetRandomName(gender, CharacterHandler));

        return sb.ToString();
    }
}
