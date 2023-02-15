using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Levels.Models.Planes;

namespace Server.Reawakened.Entities;

public abstract class MovingObjectControllerEntity<T> : SyncedEntity<T> where T : MovingObjectController
{
    protected IMovement Movement;

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

    public void Activate() => Movement?.Activate(Level.Time);

    public void Deactivate() => Movement?.Deactivate(Level.Time);
}
