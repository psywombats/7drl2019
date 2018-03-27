using System;
using System.Collections;
using System.Collections.Generic;

/**
 * A binary stat, you have it or you don't.
 */
public enum FlagStat {
    DummyFlag,
}

public class FlagStatAttribute : Stat {

}

public static class FlagStatExtensions {

    public static int Add(int value1, int value2) {
        return value1 + value2;
    }

    public static int Remove(int value1, int value2) {
        return value1 - value2;
    }
}
