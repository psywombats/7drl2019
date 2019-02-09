using System;
using UnityEngine;

public enum OrthoDir {
    [OrthoDir("North",     0,  1,  0,      0,  0,  1,      0, -1,      0)] North,
    [OrthoDir("East",      1,  0,  0,      1,  0,  0,      1,  0,      1)] East,
    [OrthoDir("South",     0, -1,  0,      0,  0, -1,      0,  1,      2)] South,
    [OrthoDir("West",     -1,  0,  0,      -1, 0,  0,     -1,  0,      3)] West,
}

public class OrthoDirAttribute : Attribute {

    // this set is in tile space
    // (xy is public for optimization)
    public IntVector2 XY;
    public int X { get { return XY.x; } }
    public int Y { get { return XY.y; } }

    // 2D screenspace
    public IntVector3 Px2D { get; private set; }
    public int Px2DX { get { return Px2D.x; } }
    public int Px2DY { get { return Px2D.y; } }
    public int Px2DZ { get { return Px2D.z; } }

    // 3D screenspace
    public IntVector3 Px3D { get; private set; }
    public int Px3DX { get { return Px3D.x; } }
    public int Px3DY { get { return Px3D.y; } }
    public int Px3DZ { get { return Px3D.z; } }

    public int Ordinal { get; private set; }
    public string DirectionName { get; private set; }

    internal OrthoDirAttribute(string directionName,
            int px2DX, int px2DY, int px2DZ,
            int px3DX, int px3DY, int px3DZ,
            int dx, int dy, int ordinal) {
        XY = new IntVector2(dx, dy);
        Px2D = new IntVector3(px2DX, px2DY, px2DZ);
        Px3D = new IntVector3(px3DX, px3DY, px3DZ);
        Ordinal = ordinal;
        DirectionName = directionName;
    }
}

public static class OrthoDirExtensions {

    public static OrthoDir DirectionOf(Vector2 vector) {
        if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y)) {
            return ((vector.x > 0) ^ (OrthoDir.East.X() > 0)) ? OrthoDir.West : OrthoDir.East;
        } else {
            return ((vector.y > 0) ^ (OrthoDir.North.Y() > 0)) ? OrthoDir.South : OrthoDir.North;
        }
    }

    // 2D space
    public static OrthoDir DirectionOfPx(Vector2 vector) {
        if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y)) {
            return ((vector.x > 0) ^ (OrthoDir.East.Px2DX() > 0)) ? OrthoDir.West : OrthoDir.East;
        } else {
            return ((vector.y > 0) ^ (OrthoDir.North.Px2DY() > 0)) ? OrthoDir.South : OrthoDir.North;
        }
    }

    // 3D space
    public static OrthoDir DirectionOfPx(Vector3 vector) {
        if (Mathf.Abs(vector.x) > Mathf.Abs(vector.z)) {
            return ((vector.x > 0) ^ (OrthoDir.East.Px3DX() > 0)) ? OrthoDir.West : OrthoDir.East;
        } else {
            return ((vector.z > 0) ^ (OrthoDir.North.Px3DZ() > 0)) ? OrthoDir.South : OrthoDir.North;
        }
    }

    public static OrthoDir Parse(string directionName) {
        foreach (OrthoDir dir in System.Enum.GetValues(typeof(OrthoDir))) {
            if (dir.DirectionName().ToLower() == directionName.ToLower()) {
                return dir;
            }
        }

        Debug.Assert(false, "Could not find orthodir matching " + directionName);
        return OrthoDir.North;
    }

    public static int X(this OrthoDir dir) { return dir.GetAttribute<OrthoDirAttribute>().X; }
    public static int Y(this OrthoDir dir) { return dir.GetAttribute<OrthoDirAttribute>().Y; }
    public static IntVector2 XY(this OrthoDir dir) { return dir.GetAttribute<OrthoDirAttribute>().XY; }

    public static int Px2DX(this OrthoDir dir) { return dir.GetAttribute<OrthoDirAttribute>().Px2DX; }
    public static int Px2DY(this OrthoDir dir) { return dir.GetAttribute<OrthoDirAttribute>().Px2DY; }
    public static int Px2DZ(this OrthoDir dir) { return dir.GetAttribute<OrthoDirAttribute>().Px2DY; }
    public static IntVector3 Px2D(this OrthoDir dir) { return new IntVector3(dir.Px2DX(), dir.Px2DY(), dir.Px2DZ()); }

    public static int Px3DX(this OrthoDir dir) { return dir.GetAttribute<OrthoDirAttribute>().Px3DX; }
    public static int Px3DY(this OrthoDir dir) { return dir.GetAttribute<OrthoDirAttribute>().Px3DY; }
    public static int Px3DZ(this OrthoDir dir) { return dir.GetAttribute<OrthoDirAttribute>().Px3DZ; }
    public static IntVector3 Px3D(this OrthoDir dir) { return new IntVector3(dir.Px3DX(), dir.Px3DY(), dir.Px3DZ()); }

    public static int Ordinal(this OrthoDir dir) { return dir.GetAttribute<OrthoDirAttribute>().Ordinal; }
    public static string DirectionName(this OrthoDir dir) { return dir.GetAttribute<OrthoDirAttribute>().DirectionName; }
}
