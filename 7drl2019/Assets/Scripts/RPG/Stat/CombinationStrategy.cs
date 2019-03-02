using UnityEngine;
using System.Collections;

/**
 * Abstracts away how stats are added to each other in a stat set.
 */
public interface CombinationStrategy {

    // default to this value for new stat sets
    float Zero();

    // called when stat2 is added to the base stat1
    float Combine(float stat1, float stat2);

    // called when stat2 is removed from the base stat1
    float Decombine(float stat1, float stat2);
}
