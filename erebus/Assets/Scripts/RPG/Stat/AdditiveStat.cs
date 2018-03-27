using System;
using System.Collections;
using System.Collections.Generic;

/**
 * Additive stat - anything straight additive used as a based in calculations.
 */
public enum AdditiveStat {
    PPower,
    SPower,
    PArmor,
    SArmor,
}

public class AdditiveStatAttribute : Stat {

}

public static class AdditiveStatExtensions {

    public static float Add(float value1, float value2) {
        return value1 + value2;
    }

    public static float Remove(float value1, float value2) {
        return value1 - value2;
    }
}
