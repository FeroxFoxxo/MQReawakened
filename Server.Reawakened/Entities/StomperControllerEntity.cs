using Server.Base.Core.Extensions;
using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using System.Reflection;

namespace Server.Reawakened.Entities;

internal class StomperControllerEntity : SyncedEntity<StomperController>
{
    public bool Active = true;

    public override string[] GetInitData(NetState netState) => new string[3] { "1.0", "1.0", Active ? "1" : "0" };

    /*
    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        Level.SendSyncEvent(syncEvent);
        netState.SendSyncEventToPlayer(syncEvent);
    }
    */
}
