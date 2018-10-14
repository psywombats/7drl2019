using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A stat set is a collection of stats of different types.
 * It can represent a modifier set (+3 str sword) or a base set (Alex, 10 str)
 * */
[System.Serializable]
public class StatSet : ISerializationCallbackReceiver {

    // serialization
    public SerialDictionary<AdditiveStat, float> serialAddStats;
    public SerialDictionary<MultiplicativeStat, float> serialMultStats;
    public SerialDictionary<FlagStat, int> serialFlagStats;

    // actual properties
    private Dictionary<AdditiveStat, float> addStats;
    private Dictionary<MultiplicativeStat, float> multStats;
    private Dictionary<FlagStat, int> flagStats;

    public StatSet() {
        addStats = new Dictionary<AdditiveStat, float>();
        multStats = new Dictionary<MultiplicativeStat, float>();
        flagStats = new Dictionary<FlagStat, int>();
        foreach (AdditiveStat statId in Enum.GetValues(typeof(AdditiveStat))) {
            addStats[statId] = 0.0f;
        }
        foreach (MultiplicativeStat statId in Enum.GetValues(typeof(MultiplicativeStat))) {
            multStats[statId] = 0.0f;
        }
        foreach (FlagStat statId in Enum.GetValues(typeof(FlagStat))) {
            flagStats[statId] = 0;
        }
    }

    public float Get(AdditiveStat stat) {
        return addStats[stat];
    }
    public float Get(MultiplicativeStat stat) {
        return multStats[stat];
    }
    public bool Is(FlagStat stat) {
        return flagStats[stat] > 0;
    }

    public void AddSet(StatSet other) {
        foreach (AdditiveStat statId in Enum.GetValues(typeof(AdditiveStat))) {
            addStats[statId] = AdditiveStatExtensions.Add(Get(statId), other.Get(statId));
        }
        foreach (MultiplicativeStat statId in Enum.GetValues(typeof(MultiplicativeStat))) {
            multStats[statId] = MultiplicativeStatExtensions.Add(Get(statId), other.Get(statId));
        }
        foreach (FlagStat statId in Enum.GetValues(typeof(FlagStat))) {
            flagStats[statId] = FlagStatExtensions.Add(flagStats[statId], other.flagStats[statId]);
        }
    }

    public void RemoveSet(StatSet other) {
        foreach (AdditiveStat statId in Enum.GetValues(typeof(AdditiveStat))) {
            addStats[statId] = AdditiveStatExtensions.Remove(Get(statId), other.Get(statId));
        }
        foreach (MultiplicativeStat statId in Enum.GetValues(typeof(MultiplicativeStat))) {
            multStats[statId] = MultiplicativeStatExtensions.Remove(Get(statId), other.Get(statId));
        }
        foreach (FlagStat statId in Enum.GetValues(typeof(FlagStat))) {
            flagStats[statId] = FlagStatExtensions.Remove(flagStats[statId], other.flagStats[statId]);
        }
    }

    // === SERIALIZATION ===

    public void OnBeforeSerialize() {
        serialAddStats = new SerialDictionary<AdditiveStat, float>(addStats);
        serialFlagStats = new SerialDictionary<FlagStat, int>(flagStats);
        serialMultStats = new SerialDictionary<MultiplicativeStat, float>(multStats);
    }

    public void OnAfterDeserialize() {
        addStats = serialAddStats.ToDictionary();
        flagStats = serialFlagStats.ToDictionary();
        multStats = serialMultStats.ToDictionary();
    }
}
