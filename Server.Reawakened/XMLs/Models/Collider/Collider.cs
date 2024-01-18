using UnityEngine;

public class Collider(string plane, float x, float y, float sx, float sy)
{
    public string Plane { get; } = plane;
    public Vector2 Position { get; } = new Vector2(x, y);
    public float Width { get; } = sx;
    public float Height { get; } = sy;
}
