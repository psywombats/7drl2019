using System;
using UnityEngine;

public enum CornerDir {
    [CornerDir("Northeast",      1,  0,  1,      0)] NW,
    [CornerDir("Southeast",      1,  0, -1,      1)] NE,
    [CornerDir("Southwest",     -1,  0,  1,      2)] SE,
    [CornerDir("Northeast",     -1,  0, -1,      3)] SW,
}

public class CornerDirAttribute : Attribute {

    // 3D screenspace
    public Vector3Int Normal { get; private set; }
    public int X { get { return Normal.x; } }
    public int Y { get { return Normal.y; } }
    public int Z { get { return Normal.z; } }

    public int Ordinal { get; private set; }
    public string DirectionName { get; private set; }

    internal CornerDirAttribute(string directionName,
            int x, int y, int z,
            int ordinal) {
        Ordinal = ordinal;
        DirectionName = directionName;
    }
}

public static class CornerDirExtensions {

    public static int X(this CornerDir dir) { return dir.GetAttribute<CornerDirAttribute>().X; }
    public static int Y(this CornerDir dir) { return dir.GetAttribute<CornerDirAttribute>().Y; }
    public static int Z(this CornerDir dir) { return dir.GetAttribute<CornerDirAttribute>().Z; }
    public static Vector3Int Normal(this CornerDir dir) { return new Vector3Int(dir.X(), dir.Y(), dir.Z()); }

    public static int Ordinal(this CornerDir dir) { return dir.GetAttribute<CornerDirAttribute>().Ordinal; }
}
