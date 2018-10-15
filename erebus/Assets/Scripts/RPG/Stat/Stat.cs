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
    public String nameShort { get; private set; }
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
        return stats[tag];
    }

    public static Stat Get(int enumIndex) {
        return Get((StatTag)enumIndex);
    }

    private static void InitializeStats() {
        stats = new Dictionary<StatTag, Stat>();
        AddStat(StatTag.MHP,        CombinationAdditive.Instance(), "MHP",      false);
        AddStat(StatTag.HP,         CombinationAdditive.Instance(), "HP",       false);
        AddStat(StatTag.RES_FIRE,   CombinationAdditive.Instance(), "O-FIRE",   true);
        AddStat(StatTag.WEAK_FIRE,  CombinationAdditive.Instance(), "X-FIRE",   true);
    }

    private static void AddStat(StatTag tag, CombinationStrategy combinator, String nameShort, bool useBinaryEditor) {
        stats[tag] = new Stat(tag, combinator, nameShort, useBinaryEditor);
    }
}
