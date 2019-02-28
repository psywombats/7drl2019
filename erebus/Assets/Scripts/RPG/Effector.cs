using System.Collections;
using UnityEngine;

/**
 * The effect of a skill/ability/spell etc. Doesn't handle the targeting, which is done by the
 * targeter, the other part of a skill. Abstract to represent melee, heals, teleporting, everything
 * under the sun. The range of the teleport, element of the damage, etc, are the serialized props.
 * Individual instaces of the effector are generated once per useage of the underlying skill.
 */
public abstract class Effector {

    /**
     * Once the targeter has done its thing and locked in the target locations or units or w/e,
     * the warhead executes and performs the actual skill on whatever the target picked up.
     */
    public abstract IEnumerator Execute(Targeter targeterInstance);

    /**
     * Can this effect be undone? Stuff like walking should be able to reverse and refund energy
     * points. If returns true, need to implement undo.
     */
    public virtual bool CanUndo() {
        return false;
    }

    /**
     * Reset the battle back to the state it was before this effect took place.
     */
    public virtual void Undo() {
        Debug.LogError(GetType() + " doesn't support undo");
    }

    /**
     * All properties common to parameters for effectors. Subclasses should subclass this usually.
     */
    public abstract class EffectorParams : ScriptableObject {

        /**
         * Return an effector of the appropriate subclass.
         */
        public abstract Effector Instantiate();
    }
}
