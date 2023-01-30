using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Models;
using Server.Reawakened.Core.Network.Protocols;
using Server.Reawakened.Levels;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Players;
using System.Xml;

namespace Protocols.System._xml__System;

public class AutoJoin : SystemProtocol
{
    public override string ProtocolName => "autoJoin";

    public LevelHandler LevelHandler { get; set; }
    public ILogger<AutoJoin> Logger { get; set; }

    public override void Run(XmlDocument xmlDoc)
    {
        var user = NetState.Get<Player>();
        var account = NetState.Get<Account>();

        Level newLevel;

        try
        {
            newLevel = LevelHandler.GetLevelFromId(0);
        }
        catch (NullReferenceException)
        {
            newLevel = null;
        }

        if (newLevel == null)
        {
            Logger.LogError("Could not find level! Returning...");
            return;
        }

        user.JoinLevel(NetState, newLevel);

        var info = user.UserInfo;

        var properties = new Dictionary<CharacterInfoHandler.ExternalProperties, string>()
        {
            { CharacterInfoHandler.ExternalProperties.Chat_Level, info.ChatLevel.ToString() },
            { CharacterInfoHandler.ExternalProperties.Gender, Enum.GetName(info.Gender) },
            { CharacterInfoHandler.ExternalProperties.Country, info.Region },
            {
                CharacterInfoHandler.ExternalProperties.Age,
                Convert.ToInt32(Math.Floor((DateTime.UtcNow - DateTime.Parse(info.DateOfBirth)).TotalDays / 365.2425))
                    .ToString()
            },
            { CharacterInfoHandler.ExternalProperties.Birthdate, info.DateOfBirth },
            { CharacterInfoHandler.ExternalProperties.AccountAge, account.Created },
            { CharacterInfoHandler.ExternalProperties.Silent, "0" },
            { CharacterInfoHandler.ExternalProperties.Uuid, info.UserId.ToString() },
            { CharacterInfoHandler.ExternalProperties.AccessRights, "2" },
            { CharacterInfoHandler.ExternalProperties.ClearCache, "0" },
            {
                CharacterInfoHandler.ExternalProperties.Now, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()
            },
            { CharacterInfoHandler.ExternalProperties.Subscriber, info.Member ? "1" : "0" }
        };

        SendXt("cx", string.Join('|', properties.Select(x => $"{(int)x.Key}|{x.Value}")));
        
        SendXt("cl", $"{user.UserInfo.LastCharacterSelected}{(user.UserInfo.Characters.Count > 0 ? "%" : "")}" +
                     string.Join('%', user.UserInfo.Characters.Select(c => c.Value.GetLightCharacterData()))
        );
    }
}
