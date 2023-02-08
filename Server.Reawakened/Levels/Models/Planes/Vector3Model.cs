﻿using UnityEngine;

namespace Server.Reawakened.Levels.Models.Planes;

public class Vector3Model
{
    public static float Distance(Vector3Model left, Vector3Model right) =>
        Mathf.Sqrt(Mathf.Pow(left.X - right.X, 2) + Mathf.Pow(left.Y - right.Y, 2) + Mathf.Pow(left.Z - right.Z, 2));

    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public override string ToString() => $"X: {X}, Y: {Y}, Z: {Z}";
}
