using System;
using System.Collections;
using UnityEngine;

public class SmiteTargeter : Targeter {

    public int range;

    protected override IEnumerator InternalExecuteRoutine(Effector effect, Result<IEnumerator> result) {
        Cursor cursor = battle.SpawnCursor(actor.location);
        SelectionGrid grid = battle.SpawnSelectionGrid();
        Func<Vector2Int, bool> rule = (Vector2Int loc) => {
            BattleEvent targetBattler = map.GetEventAt<BattleEvent>(loc);
            return targetBattler != null &&
                Vector2Int.Distance(loc, actor.location) <= range &&
                actor.align != targetBattler.unit.align;
                    
        };
        Vector2Int origin = new Vector2Int(
            (int)actorEvent.positionPx.x - range,
            (int)actorEvent.positionPx.z - range);
        grid.ConfigureNewGrid(actor.location, range, map.terrain, rule);

        Result<Vector2Int> locResult = new Result<Vector2Int>();
        while (!locResult.finished) {
            yield return battle.cursor.AwaitSelectionRoutine(locResult);
            if (!locResult.canceled) {
                if (!rule(locResult.value)) {
                    locResult.Reset();
                }
            }
        }

        battle.DespawnCursor();
        Destroy(grid.gameObject);

        if (locResult.canceled) {
            result.Cancel();
        } else {
            yield return effect.ExecuteSingleCellRoutine(result, locResult.value);
        }
    }
}
