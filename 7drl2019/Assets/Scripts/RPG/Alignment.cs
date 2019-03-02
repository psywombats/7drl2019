using System;
using UnityEngine;

public enum Alignment {
    [AlignmentAttribute("None / Unknown")] None,
    [AlignmentAttribute("Hero")] Hero,
    [AlignmentAttribute("Enemy")] Enemy,
}

public class AlignmentAttribute : Attribute {
    
    public string AlignmentName { get; private set; }

    internal AlignmentAttribute(string alignmentName) {
        this.AlignmentName = alignmentName;
    }
}

public static class AlignmentExtensions {

    public static Alignment Parse(string alignmentName) {
        foreach (Alignment align in System.Enum.GetValues(typeof(Alignment))) {
            if (align.AlignmentName().ToLower() == alignmentName.ToLower()) {
                return align;
            }
        }

        Debug.Assert(false, "Could not find alignment matching " + alignmentName);
        return Alignment.Enemy;
    }

    public static string AlignmentName(this Alignment align) { return align.GetAttribute<AlignmentAttribute>().AlignmentName; }
}
