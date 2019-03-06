using System;
using UnityEngine;

public enum SpellSchool {
    [SpellSchool(1.00f, 0.22f, 0.22f,   "Fire")] Fire,
    [SpellSchool(0.95f, 0.85f, 0.35f,   "Lightning")] Lightning,
    [SpellSchool(0.70f, 0.95f, 0.95f,   "Air")] Air,
    [SpellSchool(0.45f, 0.65f, 0.95f,   "Water")] Water,
    [SpellSchool(0.85f, 0.35f, 1.00f,   "Dark")] Dark,
}

public class SpellSchoolAttribute : Attribute {

    public string schoolName { get; private set; }
    public Color tint { get; private set; }

    internal SpellSchoolAttribute(float r, float g, float b, string schoolName) {
        this.schoolName = schoolName;
        tint = new Color(r, g, b);
    }
}

public static class SpellSchoolExtensions {

    public static string SchoolName(this SpellSchool dir) { return dir.GetAttribute<SpellSchoolAttribute>().schoolName; }
    public static Color Tint(this SpellSchool dir) { return dir.GetAttribute<SpellSchoolAttribute>().tint; }
}