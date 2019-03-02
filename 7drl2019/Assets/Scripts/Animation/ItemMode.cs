using System;
using UnityEngine;

public enum ItemMode {
    [ItemMode("disabled",   false,  0.0f    )] Disabled,
    [ItemMode("overhead",   true,   0.0f    )] Overhead,
    [ItemMode("swinging",   true,   270.0f  )] Swinging,
    [ItemMode("swung",      true,   270.0f  )] Swung,
}

public class ItemModeAttribute : Attribute {
    
    public string name { get; private set; }
    public bool show { get; private set; }
    public float rotation { get; private set; }

    internal ItemModeAttribute(string name, bool show, float rotation) {
        this.name = name;
        this.show = show;
        this.rotation = rotation;
    }
}

public static class ItemModeExtensions {

    public static ItemMode Parse(string name) {
        foreach (ItemMode mode in Enum.GetValues(typeof(ItemMode))) {
            if (mode.Name().Equals(name)) {
                return mode;
            }
        }

        Debug.Assert(false, "Could not find ItemMode matching " + name);
        return ItemMode.Disabled;
    }

    public static string Name(this ItemMode mode) { return mode.GetAttribute<ItemModeAttribute>().name; }
    public static bool Show(this ItemMode mode) { return mode.GetAttribute<ItemModeAttribute>().show; }
    public static float Rotation(this ItemMode mode) { return mode.GetAttribute<ItemModeAttribute>().rotation; }
}
