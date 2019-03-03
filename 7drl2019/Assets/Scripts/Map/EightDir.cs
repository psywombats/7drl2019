using System;
using UnityEngine;

public enum EightDir {
    [EightDir("North",      1,  0,  1,  0)] N,
    [EightDir("Northeast",  1,  0,  0,  4)] NE,
    [EightDir("East",       1,  0, -1,  1)] E,
    [EightDir("Southeast",  0,  0, -1,  5)] SE,
    [EightDir("South",     -1,  0, -1,  2)] S,
    [EightDir("Southwest", -1,  0,  0,  6)] SW,
    [EightDir("West",      -1,  0,  1,  3)] W,
    [EightDir("Northwest",  0,  0,  1,  7)] NW,
}

public class EightDirAttribute : Attribute {

    // tile space
    // (xy is public for optimization)
    public Vector2Int XY { get; private set; }

    // 3D screenspace
    public Vector3Int Px { get; private set; }
    public int PxX { get { return Px.x; } }
    public int PxY { get { return Px.y; } }
    public int PxZ { get { return Px.z; } }

    public int Ordinal { get; private set; }
    public string DirectionName { get; private set; }

    internal EightDirAttribute(string directionName,
            int px3DX, int px3DY, int px3DZ,
            int ordinal) {
        XY = new Vector2Int(px3DX, px3DZ);
        Px = new Vector3Int(px3DX, px3DY, px3DZ);
        Ordinal = ordinal;
        DirectionName = directionName;
    }
}

public static class EightDirExtensions {

    // 3D space
    public static EightDir DirectionOf(Vector2Int vector) {
        return DirectionOf(new Vector3(vector.x, 0.0f, vector.y));
    }
    public static EightDir DirectionOf(Vector3 vector) {
        float deltaN = Vector3.Distance(vector, Vector3.Project(vector, EightDir.N.Px()));
        float deltaNE = Vector3.Distance(vector, Vector3.Project(vector, EightDir.NE.Px()));
        float deltaE = Vector3.Distance(vector, Vector3.Project(vector, EightDir.E.Px()));
        float deltaSE = Vector3.Distance(vector, Vector3.Project(vector, EightDir.E.Px()));
        if (deltaN <= deltaNE && deltaN <= deltaE && deltaN <= deltaSE) {
            return Vector3.Dot(vector, EightDir.N.Px()) > 0.0f ? EightDir.N : EightDir.S;
        } else if (deltaNE <= deltaN && deltaNE <= deltaE && deltaNE <= deltaSE) {
            return Vector3.Dot(vector, EightDir.NE.Px()) > 0.0f ? EightDir.NE : EightDir.SE;
        } else if (deltaE <= deltaNE && deltaE <= deltaNE && deltaE <= deltaSE) {
            return Vector3.Dot(vector, EightDir.E.Px()) > 0.0f ? EightDir.E : EightDir.W;
        } else if (deltaSE <= deltaNE && deltaSE <= deltaNE && deltaSE <= deltaE) {
            return Vector3.Dot(vector, EightDir.SE.Px()) > 0.0f ? EightDir.SE : EightDir.SW;
        }
        return 0;
    }

    public static EightDir Parse(string directionName) {
        foreach (EightDir dir in Enum.GetValues(typeof(EightDir))) {
            if (dir.DirectionName().ToLower() == directionName.ToLower()) {
                return dir;
            }
        }
        Debug.Assert(false, "Could not find eightdir matching " + directionName);
        return 0;
    }

    public static EightDir FromCommand(InputManager.Command command) {
        switch (command) {
            case InputManager.Command.Up:           return EightDir.N;
            case InputManager.Command.UpRight:      return EightDir.NE;
            case InputManager.Command.Right:        return EightDir.E;
            case InputManager.Command.DownRight:    return EightDir.SE;
            case InputManager.Command.Down:         return EightDir.S;
            case InputManager.Command.DownLeft:     return EightDir.SW;
            case InputManager.Command.Left:         return EightDir.W;
            case InputManager.Command.UpLeft:       return EightDir.NW;
        }
        return 0;
    }

    public static Vector2Int XY(this EightDir dir) { return dir.GetAttribute<EightDirAttribute>().XY; }

    public static int PxX(this EightDir dir) { return dir.GetAttribute<EightDirAttribute>().PxX; }
    public static int PxY(this EightDir dir) { return dir.GetAttribute<EightDirAttribute>().PxY; }
    public static int PxZ(this EightDir dir) { return dir.GetAttribute<EightDirAttribute>().PxZ; }
    public static Vector3Int Px(this EightDir dir) { return dir.GetAttribute<EightDirAttribute>().Px; }

    public static int Ordinal(this EightDir dir) { return dir.GetAttribute<EightDirAttribute>().Ordinal; }
    public static string DirectionName(this EightDir dir) { return dir.GetAttribute<EightDirAttribute>().DirectionName; }
}
