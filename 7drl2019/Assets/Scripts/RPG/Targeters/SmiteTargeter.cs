using System;
using System.Collections;
using UnityEngine;

public class SmiteTargeter : Targeter {

    public int range;

    protected override IEnumerator InternalExecuteRoutine(Effector effect, Result<IEnumerator> result) {
        Cursor cursor = battle.SpawnCursor(actor.location);
        SelectionGrid grid = battle.SpawnSelectionGrid();
        Func<Vector2Int, bool> rangeRule = (Vector2Int loc) => {
            return Vector2Int.Distance(loc, actor.location) <= range;
        };
        Vector2Int origin = new Vector2Int(
            (int)actorEvent.positionPx.x - range,
            (int)actorEvent.positionPx.z - range);
        grid.ConfigureNewGrid(actor.location, range, map.terrain, rangeRule, DefaultSelectRule(effect));

        Result<Vector2Int> locResult = new Result<Vector2Int>();
        yield return battle.cursor.AwaitSelectionRoutine(locResult, DefaultSelectRule(effect));

        battle.DespawnCursor();
        Destroy(grid.gameObject);

        if (locResult.canceled) {
            result.Cancel();
        } else {
            yield return effect.ExecuteSingleCellRoutine(result, locResult.value);
        }
    }
}
