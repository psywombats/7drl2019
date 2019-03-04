using System;
using System.Collections.Generic;

/**
 * Stats are representing as instances of this class, eg STR is an instance of Stat that has an
 * additive mixin, int display, etc. Enums aren't powerful enough to do what we want in C#. Instead
 * there's a StatTag that hooks into this class.
 */
public class Stat {

    public CombinationStrategy combinator { get; private set; }
    public StatTag tag { get; private set; }
    public string nameShort { get; private set; }
    public bool useBinaryEditor { get; private set; }

    private static Dictionary<StatTag, Stat> stats;

    private Stat(StatTag tag, CombinationStrategy combinator, String nameShort, bool useBinaryEditor) {
        this.combinator = combinator;
        this.tag = tag;
        this.nameShort = nameShort;
        this.useBinaryEditor = useBinaryEditor;
    }

    public static Stat Get(StatTag tag) {
        if (stats == null) {
            InitializeStats();
        }
        if (!stats.ContainsKey(tag)) {
            return null;
        }
        return stats[tag];
    }

    public static Stat Get(int enumIndex) {
        return Get((StatTag)enumIndex);
    }

    private static void InitializeStats() {
        stats = new Dictionary<StatTag, Stat>();
        foreach (StatTag tag in Enum.GetValues(typeof(StatTag))) {
            if (tag == StatTag.None) {
                continue;
            }
            AddStat(tag, CombinationAdditive.Instance(), tag.ToString(), false);
        }
    }

    private static void AddStat(StatTag tag, CombinationStrategy combinator, string nameShort, bool useBinaryEditor) {
        stats[tag] = new Stat(tag, combinator, nameShort, useBinaryEditor);
    }
}
