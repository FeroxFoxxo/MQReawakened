namespace Server.Reawakened.Rooms.Models.Entities;

public interface IDestructible
{
    public void Destroy(Room room, int id);
}
