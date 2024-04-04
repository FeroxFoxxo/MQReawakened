using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;

namespace Protocols.External._j__LootHandler;
public class OnPlayerPassRoll : ExternalProtocol
{
    public override string ProtocolName => "jp";

    public override void Run(string[] message)
    {
        var objectId = int.Parse(message[5]);

        Player.TempData.VotedForItem.Add(objectId, false);

        var playersInRoom = Player.Room.GetPlayers();

        foreach (var player in playersInRoom)
            player.SendXt("jp", Player.UserId, objectId);

        if (playersInRoom.All(x => x.TempData.VotedForItem.ContainsKey(objectId)))
        {
            var participants = playersInRoom.Where(x => x.TempData.VotedForItem[objectId] == true).ToList();
            var winningPlayer = participants[new Random().Next(participants.Count)];

            foreach (var player in playersInRoom)
            {
                var rewardedData = new SeparatedStringBuilder('|');

                rewardedData.Append(objectId);
                rewardedData.Append(winningPlayer.UserId);

                player.SendXt("jl", rewardedData.ToString());
                player.SendUpdatedInventory();
                player.TempData.VotedForItem.Remove(objectId);
            }
        }
    }
}
