using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class StatsMemory {

    public SerialDictionary<AdditiveStat, float> additiveStats;
    public SerialDictionary<MultiplicativeStat, float> multiplicativeStats;
    public SerialDictionary<FlagStat, int> flagStats;

    public StatsMemory(StatSet stats) {
        stats.PopulateMemory(this);
    }
}
