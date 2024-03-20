using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocols.External._j__LootHandler;
public class OnPlayerWantToRoll : ExternalProtocol
{
    public override string ProtocolName => "jr";

    public override void Run(string[] message)
    {
        var objectId = int.Parse(message[5]);

        Player.TempData.VotedForItem.Add(objectId, true);

        var playersInRoom = Player.Room.Players.Values;

        foreach (var player in playersInRoom)
            player.SendXt("jr", Player.UserId, objectId);

        if (playersInRoom.All(x => x.TempData.VotedForItem.ContainsKey(objectId)))
        {
            var playerList = Player.Room.Players.Values.ToList();
            var winningPlayer = playerList[new Random().Next(playerList.Count)];

            foreach (var player in playersInRoom)
            {
                var rewardedData = new SeparatedStringBuilder('|');

                rewardedData.Append(objectId);
                rewardedData.Append(player.UserId);

                player.SendXt("jl", rewardedData.ToString());
                player.TempData.VotedForItem.Remove(objectId);
            }
        }
    }
}
