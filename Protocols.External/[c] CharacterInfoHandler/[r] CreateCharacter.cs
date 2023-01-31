using A2m.Server;
using Server.Reawakened.Core.Models;
using Server.Reawakened.Core.Network.Protocols;
using Server.Reawakened.Levels;
using Server.Reawakened.Levels.Enums;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Enums;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Services;
using Server.Reawakened.XMLs;

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
        var gender = (Gender) int.Parse(message[8]);
        var characterData = new CharacterDataModel(message[9], lowestCharacterAvailable, ServerConfig);
        var tribe = (TribeType)int.Parse(message[10]);

        var names = new [] { firstName, middleName, lastName };
        
        characterData.Allegiance = tribe;
        characterData.CharacterName = string.Join(string.Empty, names);
        characterData.UserUuid = player.UserInfo.UserId;

        // DEFAULTS
        characterData.ChatLevel = 2;
        characterData.Registered = true;
        characterData.GlobalLevel = 1;

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
            player.AddCharacter(characterData);
            player.SetLevel(ServerConfig.StartLevel, characterData.CharacterId);

            player.SendStartPlay(characterData.CharacterId, NetState, LevelHandler, WorldGraph);
        }
    }
}
