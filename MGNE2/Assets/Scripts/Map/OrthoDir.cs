using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OrthoDir {
    [OrthoDirAttribute(0, 1, 0, -1, "stepNorth")] North,
    [OrthoDirAttribute(1, 0, 1, 0, "stepEast")] East,
    [OrthoDirAttribute(0, -1, 0, 1, "stepSouth")] South,
    [OrthoDirAttribute(-1, 0, -1, 0, "stepWest")] West,
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

    public string TriggerName { get; private set; }

    internal OrthoDirAttribute(int pxDX, int pxDY, int dx, int dy, string triggerName) {
        this.XY = new IntVector2(dx, dy);
        this.PxXY = new IntVector2(pxDX, pxDY);
        this.TriggerName = triggerName;
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

    public static int X(this OrthoDir dir) { return dir.GetAttribute<OrthoDirAttribute>().X; }
    public static int Y(this OrthoDir dir) { return dir.GetAttribute<OrthoDirAttribute>().Y; }
    public static IntVector2 XY(this OrthoDir dir) { return new IntVector2(dir.X(), dir.Y()); }

    public static int PxX(this OrthoDir dir) { return dir.GetAttribute<OrthoDirAttribute>().PxX; }
    public static int PxY(this OrthoDir dir) { return dir.GetAttribute<OrthoDirAttribute>().PxY; }
    public static IntVector2 PxXY(this OrthoDir dir) { return new IntVector2(dir.PxX(), dir.PxY()); }

    public static string TriggerName(this OrthoDir dir) {
        return dir.GetAttribute<OrthoDirAttribute>().TriggerName;
    }
}
