using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A stat set is a collection of stats of different types.
 * It can represent a modifier set (+3 str sword) or a base set (Alex, 10 str)
 * */
public class StatSet : MonoBehaviour {

    private Dictionary<AdditiveStat, float> additiveStats;
    private Dictionary<MultiplicativeStat, float> multiplicativeStats;
    private Dictionary<FlagStat, int> flagStats;

    public StatSet() {
        foreach (AdditiveStat statId in Enum.GetValues(typeof(AdditiveStat))) {
            additiveStats[statId] = 0.0f;
        }
        foreach (MultiplicativeStat statId in Enum.GetValues(typeof(MultiplicativeStat))) {
            multiplicativeStats[statId] = 0.0f;
        }
        foreach (FlagStat statId in Enum.GetValues(typeof(FlagStat))) {
            flagStats[statId] = 0;
        }
    }

    public float Get(AdditiveStat stat) {
        return additiveStats[stat];
    }
    public float Get(MultiplicativeStat stat) {
        return multiplicativeStats[stat];
    }
    public bool Is(FlagStat stat) {
        return flagStats[stat] > 0;
    }

    public void AddSet(StatSet other) {
        foreach (AdditiveStat statId in Enum.GetValues(typeof(AdditiveStat))) {
            additiveStats[statId] = AdditiveStatExtensions.Add(Get(statId), other.Get(statId));
        }
        foreach (MultiplicativeStat statId in Enum.GetValues(typeof(MultiplicativeStat))) {
            multiplicativeStats[statId] = MultiplicativeStatExtensions.Add(Get(statId), other.Get(statId));
        }
        foreach (FlagStat statId in Enum.GetValues(typeof(FlagStat))) {
            flagStats[statId] = FlagStatExtensions.Add(flagStats[statId], other.flagStats[statId]);
        }
    }

    public void RemoveSet(StatSet other) {
        foreach (AdditiveStat statId in Enum.GetValues(typeof(AdditiveStat))) {
            additiveStats[statId] = AdditiveStatExtensions.Remove(Get(statId), other.Get(statId));
        }
        foreach (MultiplicativeStat statId in Enum.GetValues(typeof(MultiplicativeStat))) {
            multiplicativeStats[statId] = MultiplicativeStatExtensions.Remove(Get(statId), other.Get(statId));
        }
        foreach (FlagStat statId in Enum.GetValues(typeof(FlagStat))) {
            flagStats[statId] = FlagStatExtensions.Remove(flagStats[statId], other.flagStats[statId]);
        }
    }
}
