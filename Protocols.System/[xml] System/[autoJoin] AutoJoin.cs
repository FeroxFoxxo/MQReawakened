using A2m.Server;
using Server.Base.Accounts.Enums;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Database.Users;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
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
        Player.QuickJoinRoom(0, WorldHandler, out var _);

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
            { CharacterInfoHandler.ExternalProperties.AccessRights, player.Account.AccessLevel >= AccessLevel.Moderator ? (int)UserAccessRight.NoDictionaryChat : ServerRConfig.AccessRights },
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

    private string GetCharacterList(UserInfoModel userInfo)
    {
        var sb = new SeparatedStringBuilder('%');

        var characterIds = userInfo.CharacterIds.ToList();
        var characterData = new List<string>();

        foreach (var characterId in characterIds)
        {
            var character = CharacterHandler.GetCharacterFromId(characterId);

            characterData.Add(character.GetLightCharacterData());
        }

        sb.Append(userInfo.LastCharacterSelected);

        foreach (var character in characterData)
            sb.Append(character);

        return sb.ToString();
    }
}
