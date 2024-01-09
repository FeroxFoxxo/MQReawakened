using A2m.Server;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Enums;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Services;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._c__CharacterInfoHandler;

public class CreateCharacter : ExternalProtocol
{
    public override string ProtocolName => "cr";

    public CharacterHandler CharacterHandler { get; set; }
    public NameGenSyllables NameGenSyllables { get; set; }
    public ServerRConfig ServerConfig { get; set; }
    public WorldGraph WorldGraph { get; set; }
    public WorldHandler WorldHandler { get; set; }

    public override void Run(string[] message)
    {
        var firstName = message[5];
        var middleName = message[6];
        var lastName = message[7];
        var gender = (Gender)int.Parse(message[8]);
        var characterData = new CharacterDataModel(message[9]);
        var tribe = TribeType.Ook;

        if (ServerConfig.Is2014Client)
            tribe = (TribeType)int.Parse(message[10]);

        var names = new[] { firstName, middleName, lastName };

        if (NameGenSyllables.IsNameReserved(names, CharacterHandler))
        {
            var suggestion = NameGenSyllables.GetRandomName(gender, CharacterHandler);
            SendXt("cr", 0, suggestion[0], suggestion[1], suggestion[2]);
        }
        else if (Player.UserInfo.CharacterIds.Count > ServerConfig.MaxCharacterCount)
        {
            SendXt("cr", 1);
        }
        else
        {
            characterData.Allegiance = tribe;
            characterData.CharacterName = string.Join(string.Empty, names);
            characterData.UserUuid = Player.UserId;

            if (ServerConfig.Is2014Client)
                characterData.CompletedQuests.Add(ServerConfig.TutorialTribe2014[tribe]);

            characterData.Registered = true;

            var levelUpData = new LevelData
            {
                LevelId = ServerConfig.Is2014Client ? WorldGraph.DefaultLevel : WorldGraph.NewbZone,
                SpawnPointId = 0
            };

            var model = CharacterHandler.Create(characterData, levelUpData);

            model.SetLevelXp(1);

            Player.AddCharacter(model);

            var levelInfo = WorldHandler.GetLevelInfo(model.LevelData.LevelId);

            Player.SendStartPlay(model, levelInfo, CharacterHandler);
        }
    }
}
