using UnityEngine;
using System.Collections;

public class SkillModifier : MonoBehaviour {

    public enum Type {
        CostUp,
        CostDown,
        PagesUp,
        PagesDown,
    }

    private Type type;

    public SkillModifier(Type type) {
        this.type = type;
    }

    public string MutateName(string name) {
        switch (type) {
            case Type.CostDown:     return name + " (cheap)";
            case Type.CostUp:       return name + " (expensive)";
            case Type.PagesDown:    return name + " (concise)";
            case Type.PagesUp:      return name + " (longwinded)";
            default:                return name;
        }
    }

    public int MutateCost(int cost) {
        switch (type) {
            case Type.CostDown:     return cost / 2;
            case Type.CostUp:       return cost * 2;
            default:                return cost;
        }
    }

    public int MutatePages(int pages) {
        switch (type) {
            case Type.PagesDown:    return pages / 2;
            case Type.PagesUp:      return pages * 2;
            default:                return pages;
        }
    }
}
