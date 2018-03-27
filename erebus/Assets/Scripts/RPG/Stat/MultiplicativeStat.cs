using System;
using System.Collections;
using System.Collections.Generic;

/**
 * Multiplicative stat, for when additive stuff would be OP.
 * Classic examples: damage reduction 50% plus 50% should be 75% reduction, not 100%.
 */
public enum MultiplicativeStat {
    DamageDealtRate,
    DamageTaken,
}

public class MultiplicativeStatAttribute : Stat {

}

public class MultiplicativeStatExtensions {

    public static float Add(float value1, float value2) {
        return value1 * value2;
    }

    public static float Remove(float value1, float value2) {
        return value1 / value2;
    }
}
