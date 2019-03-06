using UnityEngine;
using System.Collections;

public class SelfTargeter : Targeter {

    protected override IEnumerator InternalExecuteRoutine(Effector effect, Result<bool> result) {
        battle.SpawnCursor(actor.location);
        Result<Vector2Int> locResult = new Result<Vector2Int>();
        yield return battle.cursor.AwaitSelectionRoutine(locResult, _ => true, null, loc => {
            return loc == actor.location;
        });
        if (locResult.canceled) {
            result.Cancel();
        } else {
            result.value = true;
        }
    }
}
