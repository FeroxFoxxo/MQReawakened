using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Levels.Models.Planes;

namespace Server.Reawakened.Entities.Abstractions;

public abstract class AbstractMovingObject<T> : SyncedEntity<T> where T : MovingObjectController
{
    public float InitialProgressRatio => EntityData.InitialProgressRatio;

    public IMovement Movement;

    public override void Update()
    {
        if (Movement == null)
            return;

        var position = Movement.GetPositionBasedOnTime(Level.Time);

        StoredEntity.GameObject.ObjectInfo.Position = new Vector3Model
        {
            X = position.x,
            Y = position.y,
            Z = position.z
        };
    }

    public override object[] GetInitData(NetState netState) => new object[]
    {
        Level.Time,
        Movement.GetBehaviorRatio(Level.Time),
        Movement.Activated
    };

    public void Activate() => Movement?.Activate(Level.Time);

    public void Deactivate() => Movement?.Deactivate(Level.Time);
}
