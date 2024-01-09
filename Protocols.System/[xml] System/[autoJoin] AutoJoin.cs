using A2m.Server;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Services;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Services;
using System.Xml;

namespace Protocols.System._xml__System;

public class AutoJoin : SystemProtocol
{
    public override string ProtocolName => "autoJoin";

    public WorldHandler WorldHandler { get; set; }
    public CharacterHandler CharacterHandler { get; set; }
    public ServerRConfig ServerRConfig { get; set; }

    public override void Run(XmlDocument xmlDoc)
    {
        Player.QuickJoinRoom(0, WorldHandler);

        SendXt("cx", GetPropertyList(GetPropertiesOfUser(Player)));
        SendXt("cl", GetCharacterList(Player.UserInfo));
    }

    private Dictionary<CharacterInfoHandler.ExternalProperties, object> GetPropertiesOfUser(Player player) =>
        new()
        {
            { CharacterInfoHandler.ExternalProperties.Chat_Level, player.UserInfo.ChatLevel },
            { CharacterInfoHandler.ExternalProperties.Gender, Enum.GetName(player.UserInfo.Gender)!.ToLower()[0] },
            { CharacterInfoHandler.ExternalProperties.Country, player.UserInfo.Region },
            {
                CharacterInfoHandler.ExternalProperties.Age,
                Convert.ToInt32(Math.Floor((DateTime.UtcNow - player.UserInfo.DateOfBirth).TotalDays / 365.2425))
            },
            { CharacterInfoHandler.ExternalProperties.Birthdate, player.UserInfo.DateOfBirth },
            { CharacterInfoHandler.ExternalProperties.AccountAge, player.Account.Created },
            { CharacterInfoHandler.ExternalProperties.Silent, player.UserInfo.ChatLevel == 0 },
            { CharacterInfoHandler.ExternalProperties.Uuid, player.Account.Id },
            { CharacterInfoHandler.ExternalProperties.AccessRights, ServerRConfig.AccessRights },
            { CharacterInfoHandler.ExternalProperties.ClearCache, ServerRConfig.ClearCache ? 1 : 0 },
            {
                CharacterInfoHandler.ExternalProperties.Now, DateTimeOffset.Now.ToUnixTimeSeconds()
            },
            { CharacterInfoHandler.ExternalProperties.Subscriber, player.UserInfo.Member ? 1 : 0 }
        };

    private static string GetPropertyList(Dictionary<CharacterInfoHandler.ExternalProperties, object> properties)
    {
        var sb = new SeparatedStringBuilder('|');

        foreach (var property in properties)
        {
            sb.Append((int)property.Key);
            sb.Append(property.Value);
        }

        return sb.ToString();
    }

    private string GetCharacterList(UserInfo userInfo)
    {
        var sb = new SeparatedStringBuilder('%');

        sb.Append(userInfo.LastCharacterSelected);

        foreach (var characterId in userInfo.CharacterIds)
        {
            var character = CharacterHandler.Get(characterId);
            sb.Append(character.Data.GetLightCharacterData());
        }

        return sb.ToString();
    }
}
