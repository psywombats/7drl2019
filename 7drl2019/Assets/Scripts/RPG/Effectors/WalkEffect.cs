using System.Collections;
using UnityEngine;

/**
 * ...You walk to the selected location.
 */
public class WalkEffect : Effector {

    public override IEnumerator ExecuteSingleCellRoutine(Result<bool> result, Vector2Int location) {
        yield return actorEvent.PathToRoutine(location);
        result.value = true;
    }
}
