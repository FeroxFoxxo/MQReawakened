using Protocols.External._i__InventoryHandler;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Rooms.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PetInformation;

namespace Protocols.External._Z__PetHandler;
public class UpdatePetMode : ExternalProtocol
{
    public override string ProtocolName => "Zm";

    public override void Run(string[] message)
    {
        Player.SendXt("Zm", Player.UserId, 1);

        var hardCodedParams = $"{Player.UserId}!{Player.Character.Data.PetItemId}!{1}!{1330}!{0}!{0}!{0}";

        var petEvent = new SyncEvent(Player.GameObjectId, SyncEvent.EventType.PetState, Player.Room.Time);

        petEvent.EventDataList.Clear();
        petEvent.EventDataList.Add(Player.CharacterId);
        petEvent.EventDataList.Add(Player.Character.Data.PetItemId);
        petEvent.EventDataList.Add((int)StateSyncType.PetStateCoopJump);
        petEvent.EventDataList.Add(hardCodedParams);

        Player.Room.SendSyncEvent(petEvent);
    }
}
