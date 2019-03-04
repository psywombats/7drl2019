using System.Collections;
using UnityEngine;

/**
 * ...You walk to the selected location.
 */
public class WalkEffect : Effector {

    public override IEnumerator ExecuteSingleCellRoutine(Result<IEnumerator> result, Vector2Int location) {
        result.value = actorEvent.PathToRoutine(location);
        yield return null;
    }
}
