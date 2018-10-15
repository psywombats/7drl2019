using UnityEngine;
using System.Collections;

public class CombinationAdditive : CombinationStrategy {

    private static CombinationAdditive instance;

    private CombinationAdditive() {
        
    }

    public static CombinationAdditive Instance() {
        if (instance == null) {
            instance = new CombinationAdditive();
        }
        return instance;
    }

    public float Combine(float stat1, float stat2) {
        return stat1 + stat2;
    }

    public float Decombine(float stat1, float stat2) {
        return stat1 - stat2;
    }

    public float Zero() {
        return 0.0f;
    }
}
