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
    
    public SerialDictionary<StatTag, float> serializedStats;
    private Dictionary<StatTag, float> stats;

    public StatSet() {
        stats = new Dictionary<StatTag, float>();
        foreach (StatTag tag in Enum.GetValues(typeof(StatTag))) {
            stats[tag] = Stat.Get(tag).combinator.Zero();
        }
    }

    public float Get(StatTag tag) {
        return stats[tag];
    }

    public bool Is(StatTag tag) {
        return stats[tag] > 0.0f;
    }

    public void AddSet(StatSet other) {
        foreach (StatTag tag in Enum.GetValues(typeof(StatTag))) {
            stats[tag] = Stat.Get(tag).combinator.Combine(stats[tag], other.stats[tag]);
        }
    }

    public void RemoveSet(StatSet other) {
        foreach (StatTag tag in Enum.GetValues(typeof(StatTag))) {
            stats[tag] = Stat.Get(tag).combinator.Decombine(stats[tag], other.stats[tag]);
        }
    }

    // === SERIALIZATION ===

    public void OnBeforeSerialize() {
        serializedStats = new SerialDictionary<StatTag, float>(stats);
    }

    public void OnAfterDeserialize() {
        stats = serializedStats.ToDictionary();
    }

    public static implicit operator StatSet(UnityEngine.Object v) {
        throw new NotImplementedException();
    }
}
