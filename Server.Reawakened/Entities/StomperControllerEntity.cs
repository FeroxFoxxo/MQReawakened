using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;

namespace Server.Reawakened.Entities;

public class StomperControllerEntity : SyncedEntity<StomperController>
{
    public bool Active = true;

    public override object[] GetInitData(NetState netState) =>
        new object[] { "1.0", "1.0", Active ? 1 : 0 };
}
