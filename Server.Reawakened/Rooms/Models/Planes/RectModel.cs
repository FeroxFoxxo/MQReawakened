namespace Server.Reawakened.Rooms.Models.Planes;

public class RectModel(float left, float top, float width, float height)
{
    public float X { get; set; } = left;
    public float Y { get; set; } = top;
    public float Width { get; set; } = width;
    public float Height { get; set; } = height;
}
