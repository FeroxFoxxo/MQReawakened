using Server.Reawakened.Database.Characters;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Enums;
using Server.Reawakened.Players.Helpers;

namespace Protocols.External._c__CharacterInfoHandler;

public class RandomName : ExternalProtocol
{
    public override string ProtocolName => "cR";

    public NameGenSyllables NameGenSyllables { get; set; }
    public CharacterHandler CharacterHandler { get; set; }

    public override void Run(string[] message) =>
        SendXt("cR", NameGenSyllables.GetRandomName((Gender)int.Parse(message[5]), CharacterHandler));
}
