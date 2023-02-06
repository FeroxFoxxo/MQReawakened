using Microsoft.Extensions.Logging;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._c__CharacterInfoHandler;

public class ChooseCharacter : ExternalProtocol
{
    public override string ProtocolName => "ch";

    public ILogger<ChooseCharacter> Logger { get; set; }
    public LevelHandler LevelHandler { get; set; }
    
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

        player.SendStartPlay(character.Data.CharacterId, NetState, LevelHandler);
    }
}
