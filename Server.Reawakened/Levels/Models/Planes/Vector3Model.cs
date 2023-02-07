namespace Server.Reawakened.Levels.Models.Planes;

public class Vector3Model
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public override string ToString() => $"X: {X}, Y: {Y}, Z: {Z}";
}
