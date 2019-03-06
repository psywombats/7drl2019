using System.Collections.Generic;
using UnityEngine;

public class RandUtils {

    public static bool Flip() {
        return Chance(0.5f);
    }

    public static bool Chance(float chance) {
        return Random.Range(0.0f, 1.0f) <= chance;
    }
}
