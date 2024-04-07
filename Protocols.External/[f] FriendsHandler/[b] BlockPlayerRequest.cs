using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Services;

namespace Protocols.External._f__FriendsHandler;
public class BlockPlayerRequest : ExternalProtocol
{
    public override string ProtocolName => "fb";

    public CharacterHandler CharacterHandler { get; set; }

    public override void Run(string[] message)
    {
        var characterName = message[5];
        var friend = CharacterHandler.GetCharacterFromName(characterName);

        if (friend != null)
        {
            var friendData = new CharacterRelationshipModel(friend.Id, Player);

            if (friendData == null)
                return;

            if (Player.PlayerContainer.GetPlayerByName(characterName) != null
                && Player.Character.Data.Friends.Contains(friend.Id))
            {
                var blockedPlayer = Player.PlayerContainer.GetPlayerByName(characterName);

                blockedPlayer.SendXt("fd", Player.CharacterName, "1");
            }
            else if (Player.Character.Data.Friends.Contains(friend.Id))
            {
                Player.Character.Data.Friends.Remove(friend.Id);
                friend.Data.Friends.Remove(Player.CharacterId);
            }

            Player.Character.Data.Blocked.Add(friend.Id);

            Player.SendXt("fb", friendData.ToString());
        }
    }
}
