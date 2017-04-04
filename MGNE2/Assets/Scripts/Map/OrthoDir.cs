using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OrthoDir {
    [OrthoDirAttribute(0, -1)] North,
    [OrthoDirAttribute(1, 0)] East,
    [OrthoDirAttribute(0, 1)] South,
    [OrthoDirAttribute(0, -1)] West,
}

public class OrthoDirAttribute : Attribute {

    public int X { get; private set; }
    public int Y { get; private set; }

    internal OrthoDirAttribute(int dx, int dy) {
        this.X = dx;
        this.Y = dy;
    }
}

public static class OrthoDirExtensions {

    public static int X(this OrthoDir dir) {
        return dir.GetAttribute<OrthoDirAttribute>().X;
    }

    public static int Y(this OrthoDir dir) {
        return dir.GetAttribute<OrthoDirAttribute>().Y;
    }
}
