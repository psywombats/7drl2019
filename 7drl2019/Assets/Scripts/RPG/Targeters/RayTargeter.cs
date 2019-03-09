using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class RayTargeter : Targeter {

    public int range = 3;
    public bool penetrates;
    public bool forTeleport;

    protected override IEnumerator InternalExecuteRoutine(Effector effect, Result<bool> result) {
        Cursor cursor = battle.SpawnCursor(actor.location);
        SelectionGrid grid = battle.SpawnSelectionGrid();

        Func<Vector2Int, bool> selectRule = (Vector2Int v) => { return IsSelected(effect, v); };
        Func<Vector2Int, bool> rangeRule = (Vector2Int v) => {
            Vector2Int otherLoc = battle.cursor.GetComponent<MapEvent>().location;
            return map.PointsAlongPath(actor.location, otherLoc).Contains(v);
        };
        Func<Vector2Int, IEnumerator> scanner = (Vector2Int loc) => {
            grid.ConfigureNewGrid(actor.location, range, map.terrain, rangeRule, selectRule);
            return null;
        };

        if (effect.TargetsHostiles()) {
            float minDist = float.MaxValue;
            BattleUnit bestUnit = null;
            foreach (BattleUnit unit in battle.units) {
                float dist = Vector2Int.Distance(unit.location, actor.location);
                cursor.GetComponent<MapEvent>().SetLocation(unit.location);
                if (unit.align != actor.align && dist < minDist && selectRule(unit.location)) {
                    bestUnit = unit;
                    minDist = dist;
                }
            }
            if (bestUnit != null) {
                cursor.GetComponent<MapEvent>().SetLocation(bestUnit.location);
            }
        }

        Func<Vector2Int, bool> constrainer = loc => Vector2.Distance(loc, actor.location) <= range;
        grid.ConfigureNewGrid(actor.location, range, map.terrain, rangeRule, selectRule);

        Result<Vector2Int> locResult = new Result<Vector2Int>();
        yield return battle.cursor.AwaitSelectionRoutine(locResult, selectRule, scanner, constrainer);

        battle.DespawnCursor();
        Destroy(grid.gameObject);

        if (locResult.canceled) {
            result.Cancel();
        } else {
            List<Vector2Int> cells = new List<Vector2Int>();
            if (penetrates) {
                foreach (Vector2Int v in map.PointsAlongPath(actor.location, locResult.value)) {
                    if (map.terrain.HeightAt(v) == map.terrain.HeightAt(actor.location)) {
                        cells.Add(v);
                    }
                }
            } else {
                cells.Add(locResult.value);
            }

            yield return effect.ExecuteCellsRoutine(cells);
            result.value = true;
        }
    }

    private bool IsSelected(Effector effect, Vector2Int toCheck) {
        Vector2Int otherLoc = battle.cursor.GetComponent<MapEvent>().location;
        if (toCheck != otherLoc) {
            return false;
        }
        if (map.terrain.HeightAt(toCheck) != map.terrain.HeightAt(actor.location)) {
            //return false;
        }
        if (!DefaultSelectRule(effect)(toCheck)) {
            return false;
        }
        if (!penetrates && !forTeleport) {
            foreach (Vector2Int v2 in map.PointsAlongPath(actor.location, otherLoc)) {
                if (v2 != toCheck && DefaultSelectRule(effect)(v2)) {
                    return false;
                }
            }
        }
        if (forTeleport) {
            foreach (Vector2Int v2 in map.PointsAlongPath(actor.location, otherLoc)) {
                if (map.GetEventAt<BattleEvent>(v2) != null) {
                    return false;
                }
            }
        }
        return true;
    }
}
