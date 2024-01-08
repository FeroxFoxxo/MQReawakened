using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Services;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._c__CharacterInfoHandler;

public class ChooseCharacter : ExternalProtocol
{
    public override string ProtocolName => "ch";

    public WorldHandler WorldHandler { get; set; }
    public ILogger<ChooseCharacter> Logger { get; set; }
    public WorldGraph WorldGraph { get; set; }
    public CharacterHandler CharacterHandler { get; set; }

    public override void Run(string[] message)
    {
        var name = message[5];
        var character = CharacterHandler.GetCharacterFromName(name);

        if (character == null)
        {
            Logger.LogError("Character of {CharacterName} for user {User} was null.", name, Player.UserId);
            return;
        }

        if (character.LevelData.LevelId == 0)
            character.SetLevel(WorldGraph.DefaultLevel, Logger);

        var levelInfo = WorldHandler.GetLevelInfo(character.LevelData.LevelId);

        Player.SendStartPlay(character, levelInfo, CharacterHandler);
    }
}
