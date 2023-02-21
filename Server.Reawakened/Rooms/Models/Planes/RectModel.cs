namespace Server.Reawakened.Rooms.Models.Planes;

public class RectModel
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }

    public RectModel(float left, float top, float width, float height)
    {
        X = left;
        Y = top;
        Width = width;
        Height = height;
    }
}
