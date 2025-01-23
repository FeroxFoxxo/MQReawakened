using A2m.Server;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Enums;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Protocols.External._c__CharacterInfoHandler;

public class CreateCharacter : ExternalProtocol
{
    public override string ProtocolName => "cr";

    public CharacterHandler CharacterHandler { get; set; }
    public NameGenSyllables NameGenSyllables { get; set; }
    public ServerRConfig ServerConfig { get; set; }
    public WorldGraph WorldGraph { get; set; }
    public WorldHandler WorldHandler { get; set; }
    public EventPrefabs EventPrefabs { get; set; }

    public override void Run(string[] message)
    {
        // Early 2012 doesn't split the names
        var firstName = message[5];
        var middleName = string.Empty;
        var lastName = string.Empty;

        Gender gender;
        CharacterDbEntry characterEntry;

        var tribe = TribeType.Invalid;

        if (ServerConfig.GameVersion >= GameVersion.vPets2012)
        {
            middleName = message[6];
            lastName = message[7];
            gender = (Gender)int.Parse(message[8]);
            characterEntry = new CharacterDbEntry(message[9]);
        }
        else
        {
            gender = (Gender)int.Parse(message[6]);
            characterEntry = new CharacterDbEntry(message[7]);
        }

        if (ServerConfig.GameVersion >= GameVersion.vEarly2014)
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
            characterEntry.Allegiance = tribe;
            characterEntry.CharacterName = string.Join(string.Empty, names);
            characterEntry.UserUuid = Player.UserId;

            if (ServerConfig.GameVersion >= GameVersion.vEarly2014)
                characterEntry.CompletedQuests.Add(ServerConfig.TutorialTribe2014[tribe]);

            characterEntry.Registered = true;

            characterEntry.LevelId = ServerConfig.GameVersion >= GameVersion.vEarly2014 ? WorldGraph.DefaultLevel : WorldGraph.NewbZone;
            characterEntry.SpawnPointId = string.Empty;

            CharacterHandler.Add(characterEntry);

            var characterData = CharacterHandler.GetCharacterFromName(characterEntry.CharacterName);

            characterData.SetLevelXp(1, ServerConfig);

            Player.AddCharacter(characterData);

            var levelInfo = WorldHandler.GetLevelInfo(characterData.LevelId);

            Player.SendStartPlay(characterData, levelInfo, EventPrefabs);
        }
    }
}
