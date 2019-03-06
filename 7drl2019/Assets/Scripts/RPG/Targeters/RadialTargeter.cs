using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class RadialTargeter : Targeter {

    public float radius = 1.0f;

    protected override IEnumerator InternalExecuteRoutine(Effector effect, Result<bool> result) {
        Func<Vector2Int, bool> rangeRule = (Vector2Int loc) => {
            return Vector2Int.Distance(loc, actor.location) <= radius;
        };

        Vector2Int origin = new Vector2Int(
            (int)actorEvent.positionPx.x - Mathf.CeilToInt(radius),
            (int)actorEvent.positionPx.z - Mathf.CeilToInt(radius));

        SelectionGrid grid = battle.SpawnSelectionGrid();
        grid.ConfigureNewGrid(actor.location, Mathf.CeilToInt(radius), map.terrain, rangeRule, DefaultSelectRule(effect));

        Result<Vector2Int> locResult = new Result<Vector2Int>();
        battle.SpawnCursor(actor.location);
        yield return battle.cursor.AwaitSelectionRoutine(locResult, _ => true, null, loc => {
            return loc == actor.location;
        });
        battle.DespawnCursor();
        Destroy(grid.gameObject);

        if (locResult.canceled) {
            result.Cancel();
        } else {
            List<Vector2Int> cells = new List<Vector2Int>();
            int r = Mathf.CeilToInt(radius);
            for (int y = locResult.value.y - r; y <= locResult.value.y + r; y += 1) {
                for (int x = locResult.value.x - r; x <= locResult.value.x + r; x += 1) {
                    Vector2Int cell = new Vector2Int(x, y);
                    if (Vector2Int.Distance(cell, locResult.value) <= radius) {
                        cells.Add(cell);
                    }
                }
            }
            yield return effect.ExecuteCellsRoutine(cells);
            result.value = true;
        }
    }
}
