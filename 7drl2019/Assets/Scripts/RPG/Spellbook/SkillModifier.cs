using UnityEngine;
using System.Collections;

public class SkillModifier {

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
            case Type.PagesDown:    return Mathf.CeilToInt(pages * 0.5f);
            case Type.PagesUp:      return Mathf.CeilToInt(pages * 1.5f);
            default:                return pages;
        }
    }

    public string DescriptiveString() {
        switch (type) {
            case Type.CostDown:     return "It costs less resources to cast than average.";
            case Type.CostUp:       return "It costs mores resources to cast than average.";
            case Type.PagesDown:    return "It is written in obnoxious, tiny font on a relatively few number of pages.";
            case Type.PagesUp:      return "It is written in obnoxious, flowery font on more pages than usual.";
        }
        return "?>";
    }
}
