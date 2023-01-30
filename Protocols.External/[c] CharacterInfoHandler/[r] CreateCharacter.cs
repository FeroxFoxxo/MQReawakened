using A2m.Server;
using Server.Reawakened.Characters.Models;
using Server.Reawakened.Core.Models;
using Server.Reawakened.Core.Network.Protocols;
using Server.Reawakened.Levels;
using Server.Reawakened.Levels.Enums;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Services;
using Server.Reawakened.XMLs;
using System.Globalization;

namespace Protocols.External._c__CharacterInfoHandler;

public class CreateCharacter : ExternalProtocol
{
    public override string ProtocolName => "cr";

    public const char LevelDelimiter = '!';

    public UserInfoHandler UserInfoHandler { get; set; }
    public NameGenSyllables NameGenSyllables { get; set; }
    public ServerConfig ServerConfig { get; set; }
    public LevelHandler LevelHandler { get; set; }
    public WorldGraph WorldGraph { get; set; }

    public override void Run(string[] message)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        var firstName = message[5];
        var middleName = message[6];
        var lastName = message[7];
        var gender = int.Parse(message[8]);
        var characterData = new CharacterDataModel(message[9], ServerConfig);
        var tribe = (TribeType)int.Parse(message[10]);

        var names = new [] { firstName, middleName, lastName };

        var player = NetState.Get<Player>();

        characterData.Allegiance = tribe;
        characterData.CharacterName = string.Join(' ', names);
        characterData.UserUuid = player.UserInfo.UserId;

        // DEFAULTS
        characterData.ChatLevel = 2;
        characterData.Registered = true;

        if (NameGenSyllables.IsNameReserved(names, UserInfoHandler))
        {
            var suggestion = NameGenSyllables.GetRandomName(gender, UserInfoHandler);
            SendXt("cr", "0", suggestion[0], suggestion[1], suggestion[2]);
        }
        else if (player.UserInfo.Characters.Count > ServerConfig.MaxCharacterCount)
        {
            SendXt("cr", "1");
        }
        else
        {
            player.UserInfo.Characters.Add(characterData.CharacterId, characterData);
            player.CurrentCharacter = characterData.CharacterId;

            var error = string.Empty;
            var levelName = string.Empty;
            var surroundingLevels = string.Empty;

            Level level = null;

            try
            {
                level = LevelHandler.GetLevelFromId(ServerConfig.StartLevel);
                levelName = level.LevelData.Name;
                surroundingLevels = string.Join(LevelDelimiter,
                    WorldGraph.GetLevelWorldGraphNodes(level.LevelData.LevelId)
                        .Where(x => x.ToLevelID != x.LevelID)
                        .Select(x => WorldGraph.GetInfoLevel(x.ToLevelID).Name)
                        .Distinct()
                    );
            }
            catch (Exception e)
            {
                error = e.Message;
            }

            SendXt("lw", error, levelName, surroundingLevels);
            level?.SendCharacterInfoData(NetState, player, CharacterInfoType.Detailed);
        }
    }
}
