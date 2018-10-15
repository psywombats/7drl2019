using System;
using System.Collections;
using System.Collections.Generic;

/**
 * Stats are representing as instances of this class, eg STR is an instance of Stat that has an
 * additive mixin, int display, etc. Enums aren't powerful enough to do what we want in C#. Instead
 * there's a StatTag that hooks into this class.
 */
public class Stat {

    public CombinationStrategy combinator { get; private set; }
    public StatTag tag { get; private set; }

    private static Dictionary<StatTag, Stat> stats;

    private Stat(StatTag tag, CombinationStrategy combinator) {
        this.combinator = combinator;
        this.tag = tag;
    }

    public static Stat Get(StatTag tag) {
        if (stats == null) {
            InitializeStats();
        }
        return stats[tag];
    }

    private static void InitializeStats() {
        AddStat(StatTag.MHP,    CombinationAdditive.Instance());
        AddStat(StatTag.HP,     CombinationAdditive.Instance());
    }

    private static void AddStat(StatTag tag, CombinationStrategy combinator) {
        stats[tag] = new Stat(tag, combinator);
    }
}
