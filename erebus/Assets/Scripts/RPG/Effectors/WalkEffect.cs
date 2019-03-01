using System.Collections;
using UnityEngine;

/**
 * ...You walk to the selected location.
 */
public class WalkEffect : Effector {

    private Vector2Int target;

    public override IEnumerator ExecuteSingleCellRoutine(Result<bool> result, Vector2Int location) {
        yield return mapEvent.PathToRoutine(location);
        result.value = true;
    }
}
