using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;

namespace Server.Reawakened.Entities;

internal class StomperControllerEntity : SyncedEntity<StomperController>
{
    public bool Active = true;

    public override string[] GetInitData(NetState netState) =>
        new [] { "1.0", "1.0", Active ? "1" : "0" };
}
