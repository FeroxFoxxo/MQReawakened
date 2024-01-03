using Server.Reawakened.Entities.Components;
using Server.Reawakened.Players.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Reawakened.Players.Models.Character;
public class ArenaModel
{
    public bool StartArena { get; set; }
    public bool HasStarted { get; set; }    
    public int FirstPlayerId { get; set; }
    public int SecondPlayerId { get; set; }
    public int ThirdPlayerId { get; set; }
    public int FourthPlayerId { get; set; }
    public Dictionary<string, float> BestTimeForLevel { get; set; } = [];

    public void SetCharacterIds(Player player, IEnumerable<Player> players)
    {
        var playersInGroup = players.ToArray();
        player.TempData.ArenaModel.FirstPlayerId = playersInGroup.Length > 0 ? playersInGroup[0].GameObjectId : 0;
        player.TempData.ArenaModel.SecondPlayerId = playersInGroup.Length > 1 ? playersInGroup[1].GameObjectId : 0;
        player.TempData.ArenaModel.ThirdPlayerId = playersInGroup.Length > 2 ? playersInGroup[2].GameObjectId : 0;
        player.TempData.ArenaModel.FourthPlayerId = playersInGroup.Length > 3 ? playersInGroup[3].GameObjectId : 0;
    }
}
