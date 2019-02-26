using System;
using UnityEngine;

public enum ArmMode {
    [ArmMode("disabled",    false,      0,  0,      5)] Disabled,
    [ArmMode("neutral",     true,       16, 9,      5)] Neutral,
    [ArmMode("overhead",    true,       17, 15,     6)] Overhead,
    [ArmMode("lowered",     true,       21, 10,     7)] Lowered,
    [ArmMode("raised",      true,       22, 12,     8)] Raised,
}

public class ArmModeAttribute : Attribute {

    public string name { get; private set; }
    public bool show { get; private set; }
    public Vector2Int itemAnchor { get; private set; }
    public int frameIndex;

    internal ArmModeAttribute(string name, bool show, int anchorX, int anchorY, int frameIndex) {
        this.name = name;
        this.show = show;
        this.frameIndex = frameIndex;
        itemAnchor = new Vector2Int(anchorX, anchorY);
    }
}

public static class ArmModeExtensions {

    public static ArmMode Parse(string name) {
        foreach (ArmMode mode in Enum.GetValues(typeof(ArmMode))) {
            if (mode.Name().Equals(name)) {
                return mode;
            }
        }

        Debug.Assert(false, "Could not find ArmMode matching " + name);
        return ArmMode.Disabled;
    }

    public static string Name(this ArmMode mode) { return mode.GetAttribute<ArmModeAttribute>().name; }
    public static bool Show(this ArmMode mode) { return mode.GetAttribute<ArmModeAttribute>().show; }
    public static Vector2Int ItemAnchor(this ArmMode mode) { return mode.GetAttribute<ArmModeAttribute>().itemAnchor; }
    public static int FrameIndex(this ArmMode mode) { return mode.GetAttribute<ArmModeAttribute>().frameIndex; }
}
