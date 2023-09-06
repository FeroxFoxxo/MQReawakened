﻿using A2m.Server;
using Microsoft.Extensions.Logging;
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
using Web.Launcher.Models;

namespace Protocols.External._c__CharacterInfoHandler;

public class CreateCharacter : ExternalProtocol
{
    public override string ProtocolName => "cr";

    public UserInfoHandler UserInfoHandler { get; set; }
    public NameGenSyllables NameGenSyllables { get; set; }
    public ServerRConfig ServerConfig { get; set; }
    public WorldGraph WorldGraph { get; set; }
    public WorldHandler WorldHandler { get; set; }
    public ILogger<CreateCharacter> Logger { get; set; }
    public LauncherRwConfig LauncherRwConfig { get; set; }

    public override void Run(string[] message)
    {
        var lowestCharacterAvailable = Enumerable.Range(1, ServerConfig.MaxCharacterCount)
            .Except(Player.UserInfo.Characters.Keys).Min();

        var firstName = message[5];
        var middleName = message[6];
        var lastName = message[7];
        var gender = (Gender)int.Parse(message[8]);
        var characterData = new CharacterDataModel(message[9], lowestCharacterAvailable, ServerConfig);
        var tribe = TribeType.Crossroads;

        if (LauncherRwConfig.Is2014Client)
            tribe = (TribeType)int.Parse(message[10]);

        var names = new[] { firstName, middleName, lastName };

        if (NameGenSyllables.IsNameReserved(names, UserInfoHandler))
        {
            var suggestion = NameGenSyllables.GetRandomName(gender, UserInfoHandler);
            SendXt("cr", 0, suggestion[0], suggestion[1], suggestion[2]);
        }
        else if (Player.UserInfo.Characters.Count > ServerConfig.MaxCharacterCount)
        {
            SendXt("cr", 1);
        }
        else
        {
            characterData.Allegiance = tribe;
            characterData.CharacterName = string.Join(string.Empty, names);
            characterData.UserUuid = Player.UserId;

            characterData.Registered = true;

            var model = new CharacterModel
            {
                Data = characterData,
                LevelData = new LevelData
                {
                    LevelId = WorldGraph.DefaultLevel,
                    PortalId = 0,
                    SpawnPointId = 0
                }
            };

            model.SetLevelXp(1);

            Player.AddCharacter(model);

            var levelInfo = WorldHandler.GetLevelInfo(model.LevelData.LevelId);

            Player.SendStartPlay(model, levelInfo);
        }
    }
}
