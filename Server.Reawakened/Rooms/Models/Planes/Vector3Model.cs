using Microsoft.Extensions.Logging;
using UnityEngine;

namespace Server.Reawakened.Rooms.Models.Planes;

public class Vector3Model
{
    public float X { get; private set; }
    public float Y { get; private set; }
    public float Z { get; private set; }

    private readonly string _id;
    private readonly Room _room;

    public Vector3Model(float x, float y, float z, string id, Room room)
    {
        InternalSetPosition(x, y, z);
        _id = id;
        _room = room;
    }

    public void SetPosition(Vector3 position) => SetPosition(position.x, position.y, position.z);
    public void SetPosition(vector3 position) => SetPosition(position.x, position.y, position.z);

    public void SetPosition(float x, float y, float z)
    {
        if (_room == null)
        {
            InternalSetPosition(x, y, z);
            return;
        }

        var collider = _room.GetColliderById(_id);

        if (collider == null)
        {
            InternalSetPosition(x, y, z);
            return;
        }

        collider.Position.x += x - X;
        collider.Position.y += y - Y;
        collider.Position.z += z - Z;

        InternalSetPosition(x, y, z);
    }

    private void InternalSetPosition(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public void SetPositionViaPlane(string parentPlane, string prefabName, Microsoft.Extensions.Logging.ILogger logger)
    {
        switch (parentPlane)
        {
            case "Plane1":
                Z = 20;
                break;
            case "Plane0":
                Z = 0;
                break;
            default:
                logger.LogError("Unknown plane: '{Plane}' for prefab {Name}", parentPlane, prefabName);
                break;
        }
    }

    public static float Distance(Vector3Model left, Vector3Model right) =>
        Mathf.Sqrt(Mathf.Pow(left.X - right.X, 2) + Mathf.Pow(left.Y - right.Y, 2) + Mathf.Pow(left.Z - right.Z, 2));

    public override string ToString() => $"X: {X}, Y: {Y}, Z: {Z}";
}
