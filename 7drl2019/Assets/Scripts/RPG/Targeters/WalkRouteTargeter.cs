﻿using UnityEngine;
using System.Collections;
using System;

public class WalkRouteTargeter : Targeter {

    private Vector2Int targetLocation;

    protected override IEnumerator InternalExecuteRoutine(Effector effect, Result<IEnumerator> result) {
        Cursor cursor = battle.SpawnCursor(actor.location);
        SelectionGrid grid = battle.SpawnSelectionGrid();
        int range = (int)actor.Get(StatTag.MOVE);
        Func<Vector2Int, bool> rule = (Vector2Int loc) => {
            if (loc == actor.location) {
                return false;
            }
            return map.FindPath(actorEvent, loc, range + 1) != null;
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
                    Global.Instance().Audio.PlaySFX(SFX.error);
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
