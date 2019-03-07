using System.Collections.Generic;
using UnityEngine;

public class RandUtils {

    public static bool Flip() {
        return Chance(0.5f);
    }

    public static bool Chance(float chance) {
        return Random.Range(0.0f, 1.0f) <= chance;
    }

    public static void Shuffle<T>(List<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = Random.Range(0, n);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
