using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * The effect of a skill/ability/spell etc. Doesn't handle the targeting, which is done by the
 * targeter, the other part of a skill. Abstract to represent melee, heals, teleporting, everything
 * under the sun. The range of the teleport, element of the damage, etc, are the serialized props.
 * Individual instaces of the effector are generated once per useage of the underlying skill.
 */
public abstract class Effector : ActorScriptableObject {
    
    public virtual bool TargetsHostiles() {
        return true;
    }

    public virtual bool AcceptsEmptyGrids() {
        return false;
    }

    public virtual bool AcceptsFullGrids() {
        return true;
    }

    // === TARGETER HOOKUPS ========================================================================
    // subclasses override as they support

    public virtual IEnumerator ExecuteCellsRoutine(List<Vector2Int> locations) {
        Debug.LogError(GetType() + " does not support single cell targeters");
        yield return null;
    }

    public virtual IEnumerator ExecuteSingleCellRoutine(Vector2Int location) {
        yield return ExecuteCellsRoutine(new List<Vector2Int>() { location });
    }

    public virtual IEnumerator ExecuteDirectionRoutine(EightDir dir) {
        Vector2Int target = actor.location + dir.XY();
        yield return ExecuteSingleCellRoutine(target);
    }
}
