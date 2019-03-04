using System.Collections;
using UnityEngine;

/**
 * The effect of a skill/ability/spell etc. Doesn't handle the targeting, which is done by the
 * targeter, the other part of a skill. Abstract to represent melee, heals, teleporting, everything
 * under the sun. The range of the teleport, element of the damage, etc, are the serialized props.
 * Individual instaces of the effector are generated once per useage of the underlying skill.
 */
public abstract class Effector : ActorScriptableObject {

    /**
     * Stuff like walking should be able to reverse and refund energy points. The result will be
     * false if the action is actually not undoable, true if it succeeded.
     */
    public virtual IEnumerator Undo(Result<bool> undoableResult) {
        undoableResult.value = false;
        yield return null;
    }

    // === TARGETER HOOKUPS ========================================================================
    // subclasses override as they support

    public virtual IEnumerator ExecuteSingleCellRoutine(Result<IEnumerator> result, Vector2Int location) {
        Debug.LogError(GetType() + " does not support single cell targeters");
        result.Cancel();
        yield return null;
    }

    public virtual IEnumerator ExecuteDirectionRoutine(Result<IEnumerator> result, EightDir dir) {
        Debug.LogError(GetType() + " does not support single dir targeters");
        result.Cancel();
        yield return null;
    }
}
