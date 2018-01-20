using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OrthoDir {
    [OrthoDirAttribute("North",     0, 1, 0, -1,     0)] North,
    [OrthoDirAttribute("East",      1, 0, 1, 0,      1)] East,
    [OrthoDirAttribute("South",     0, -1, 0, 1,     2)] South,
    [OrthoDirAttribute("West",      -1, 0, -1, 0,    3)] West,
}

public class OrthoDirAttribute : Attribute {

    // this set is in tile space
    public IntVector2 XY { get; private set; }
    public int X { get { return XY.x; } }
    public int Y { get { return XY.y; } }

    // and this one's in screen space
    public IntVector2 PxXY { get; private set; }
    public int PxX { get { return PxXY.x; } }
    public int PxY { get { return PxXY.y; } }

    public int Ordinal { get; private set; }
    public string DirectionName { get; private set; }

    internal OrthoDirAttribute(string directionName, int pxDX, int pxDY, int dx, int dy, int ordinal) {
        this.XY = new IntVector2(dx, dy);
        this.PxXY = new IntVector2(pxDX, pxDY);
        this.Ordinal = ordinal;
        this.DirectionName = directionName;
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

    public static OrthoDir DirectionOfPx(Vector2 vector) {
        if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y)) {
            return ((vector.x > 0) ^ (OrthoDir.East.PxX() > 0)) ? OrthoDir.West : OrthoDir.East;
        } else {
            return ((vector.y > 0) ^ (OrthoDir.North.PxY() > 0)) ? OrthoDir.South : OrthoDir.North;
        }
    }

    public static OrthoDir Parse(string directionName) {
        foreach (OrthoDir dir in System.Enum.GetValues(typeof(OrthoDir))) {
            if (dir.DirectionName().ToLower() == directionName.ToLower()) {
                return dir;
            }
        }

        // TODO: assert
        return OrthoDir.North;
    }

    public static int X(this OrthoDir dir) { return dir.GetAttribute<OrthoDirAttribute>().X; }
    public static int Y(this OrthoDir dir) { return dir.GetAttribute<OrthoDirAttribute>().Y; }
    public static IntVector2 XY(this OrthoDir dir) { return new IntVector2(dir.X(), dir.Y()); }

    public static int PxX(this OrthoDir dir) { return dir.GetAttribute<OrthoDirAttribute>().PxX; }
    public static int PxY(this OrthoDir dir) { return dir.GetAttribute<OrthoDirAttribute>().PxY; }
    public static IntVector2 PxXY(this OrthoDir dir) { return new IntVector2(dir.PxX(), dir.PxY()); }

    public static int Ordinal(this OrthoDir dir) { return dir.GetAttribute<OrthoDirAttribute>().Ordinal; }
    public static string DirectionName(this OrthoDir dir) { return dir.GetAttribute<OrthoDirAttribute>().DirectionName; }
}
