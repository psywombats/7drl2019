using System;
using System.Collections;
using UnityEngine;

public class SmiteTargeter : Targeter {

    public int range = 0;
    public float radius = 1.0f;

    protected override IEnumerator InternalExecuteRoutine(Effector effect, Result<IEnumerator> result) {
        Cursor cursor = battle.SpawnCursor(actor.location);
        SelectionGrid grid = battle.SpawnSelectionGrid();

        Func<Vector2Int, bool> selectRule;
        Func<Vector2Int, IEnumerator> scanner = null;
        Func<Vector2Int, bool> rangeRule = (Vector2Int loc) => {
            return Vector2Int.Distance(loc, actor.location) <= range;
        };
        if (radius > 1) {
            selectRule = (Vector2Int loc) => {
                return Vector2Int.Distance(cursor.GetComponent<MapEvent>().location, loc) <= radius;
            };
            scanner = (Vector2Int loc) => {
                grid.ConfigureNewGrid(actor.location, range + Mathf.CeilToInt(radius), map.terrain, rangeRule, selectRule);
                return null;
            };
        } else {
            selectRule = DefaultSelectRule(effect);
        }
        
        if (effect.TargetsHostiles()) {
            float minDist = float.MaxValue;
            BattleUnit bestUnit;
            foreach (BattleUnit unit in battle.units) {
                float dist = Vector2Int.Distance(unit.location, actor.location);
                if (unit.align != actor.align && dist < minDist && selectRule(unit.location)) {
                    bestUnit = unit;
                    minDist = dist;
                }
                if (unit != null) {
                    cursor.GetComponent<MapEvent>().SetLocation(unit.location);
                }
            }
        }
        
        Vector2Int origin = new Vector2Int(
            (int)actorEvent.positionPx.x - (range + Mathf.CeilToInt(radius) - 1),
            (int)actorEvent.positionPx.z - (range + Mathf.CeilToInt(radius) - 1));
        
        Func<Vector2Int, bool> constrainer = loc => Vector2.Distance(loc, actor.location) <= range;
        grid.ConfigureNewGrid(actor.location, range, map.terrain, rangeRule, selectRule);

        Result<Vector2Int> locResult = new Result<Vector2Int>();
        yield return battle.cursor.AwaitSelectionRoutine(locResult, DefaultSelectRule(effect), scanner, constrainer);

        battle.DespawnCursor();
        Destroy(grid.gameObject);

        if (locResult.canceled) {
            result.Cancel();
        } else {
            yield return effect.ExecuteSingleCellRoutine(result, locResult.value);
        }
    }
}
