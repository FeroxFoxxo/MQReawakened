using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Network.Protocols;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Players;

namespace Protocols.External._c__CharacterInfoHandler;

public class ChooseCharacter : ExternalProtocol
{
    public ILogger<ChooseCharacter> Logger { get; set; }

    public LevelHandler LevelHandler { get; set; }

    public override string ProtocolName => "ch";

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();

        var name = message[5];
        var character = player.GetCharacterFromName(name);

        if (character == null)
        {
            Logger.LogError("Character of {CharacterName} for user {User} was null.",
                name, player.UserInfo.UserId);
            return;
        }
        
        player.SendStartPlay(character.CharacterId, NetState, LevelHandler);
    }
}
