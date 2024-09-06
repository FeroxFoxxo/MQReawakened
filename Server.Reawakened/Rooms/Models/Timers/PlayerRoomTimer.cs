namespace Server.Reawakened.Rooms.Models.Timers;
public class PlayerRoomTimer : PlayerTimer
{
    public override bool IsValid() => base.IsValid() && Player.Room != null && Player.Room.IsOpen;
}
