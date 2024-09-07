using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.GameObjects.Platforms;

public class CollapsingPlatformComp : Component<CollapsingPlatform>
{
    public float ShakingTime => ComponentData.ShakingTime;
    public float RegenTime => ComponentData.RegenTime;

    private float _timer;
    public bool IsBroken = false;

    public override void InitializeComponent()
    {
        Position.SetPosition(Position.X, Position.Y + Rectangle.Y, Position.Z);

        var box = new Rect(Rectangle.X, Rectangle.Y, Rectangle.Width, Rectangle.Height);
        var position = new Vector3(Position.X, Position.Y, Position.Z);

        Room.AddCollider(new BreakableCollider(Id, position, box, ParentPlane, Room, false));
    }

    public override void Update()
    {
        if (IsBroken && _timer <= Room.Time)
        {
            IsBroken = false;
            Collapse(true);
        }
        base.Update();
    }

    public void Collapse(bool regen)
    {
        if (IsBroken) return;

        IsBroken = false;
        var syncEvent = new SyncEvent(Id.ToString(), SyncEvent.EventType.CollapsingPlatform, Room.Time);
        if (regen)
        {
            syncEvent.EventDataList.Add(Room.Time);
            syncEvent.EventDataList.Add(0);
            _timer = Room.Time + 1;
        }
        else
        {
            syncEvent.EventDataList.Add(Room.Time + ShakingTime);
            syncEvent.EventDataList.Add(1);
            _timer = Room.Time + ShakingTime + RegenTime;
            IsBroken = true;
        }

        var collapseEvent = new CollapsingPlatform_SyncEvent(syncEvent);
        Room.SendSyncEvent(collapseEvent);
    }
}
