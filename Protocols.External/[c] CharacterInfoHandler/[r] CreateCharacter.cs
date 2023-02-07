using A2m.Server;
using Server.Reawakened.Configs;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Enums;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Services;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._c__CharacterInfoHandler;

public class CreateCharacter : ExternalProtocol
{
    public override string ProtocolName => "cr";

    public UserInfoHandler UserInfoHandler { get; set; }
    public NameGenSyllables NameGenSyllables { get; set; }
    public ServerConfig ServerConfig { get; set; }
    public LevelHandler LevelHandler { get; set; }
    public WorldGraph WorldGraph { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();

        var lowestCharacterAvailable = Enumerable.Range(1, ServerConfig.MaxCharacterCount)
            .Except(player.UserInfo.Characters.Keys).Min();

        var firstName = message[5];
        var middleName = message[6];
        var lastName = message[7];
        var gender = (Gender)int.Parse(message[8]);
        var characterData = new CharacterDataModel(message[9], lowestCharacterAvailable, ServerConfig, player.UserInfo);
        var tribe = (TribeType)int.Parse(message[10]);

        var names = new[] { firstName, middleName, lastName };

        if (NameGenSyllables.IsNameReserved(names, UserInfoHandler))
        {
            var suggestion = NameGenSyllables.GetRandomName(gender, UserInfoHandler);
            SendXt("cr", 0, suggestion[0], suggestion[1], suggestion[2]);
        }
        else if (player.UserInfo.Characters.Count > ServerConfig.MaxCharacterCount)
        {
            SendXt("cr", 1);
        }
        else
        {
            characterData.Allegiance = tribe;
            characterData.CharacterName = string.Join(string.Empty, names);
            characterData.UserUuid = player.UserInfo.UserId;

            // DEFAULTS
            characterData.Registered = true;
            characterData.LevelUp(1);

            characterData.CurrentLife = characterData.MaxLife;

            player.AddCharacter(new CharacterModel
            {
                Data = characterData,
                Level = WorldGraph.ClockTowerId
            });
            player.SendStartPlay(characterData.CharacterId, NetState, LevelHandler);
        }
    }
}
