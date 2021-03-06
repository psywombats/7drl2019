﻿using System;
using System.Collections;
using UnityEngine;

/**
 * Abstract to cover single-empty-square-at-range vs one-direction. Serialized props on each
 * instance are stuff like range, radius... mostly simple.
 **/
public abstract class Targeter : ActorScriptableObject {

    /**
     * Acquire the targets, pass them to the effector via the appropriate method.
     */
    public IEnumerator ExecuteRoutine(Effector effect, Result<bool> executeResult) {
        yield return InternalExecuteRoutine(effect, executeResult);
    }

    public abstract IEnumerator TryAIUse(AIController ai, Effector effect);

    protected abstract IEnumerator InternalExecuteRoutine(Effector effect, Result<bool> result);

    protected Func<Vector2Int, bool> DefaultSelectRule(Effector effect) {
        return (Vector2Int loc) => {
            if (loc.x == map.size.x - 1 || loc.y == map.size.y - 1) return false;
            BattleEvent targetBattler = map.GetEventAt<BattleEvent>(loc);
            if (!actorEvent.CanPassAt(loc) && map.GetEventAt<BattleEvent>(loc) == null) {
                return false;
            }
            return DefaultUnitRule(effect)(targetBattler != null ? targetBattler.unit : null);
        };
    }

    protected Func<BattleUnit, bool> DefaultUnitRule(Effector effect) {
        return (BattleUnit targetBattler) => {
            if (!effect.AcceptsEmptyGrids()) {
                if (targetBattler == null) {
                    return false;
                }
                if (effect.TargetsHostiles()) {
                    return actor.align != targetBattler.unit.align;
                } else {
                    return actor.align == targetBattler.unit.align;
                }
            } else if (!effect.AcceptsFullGrids()) {
                return targetBattler == null;
            } else {
                return true;
            }
        };
    }
}
