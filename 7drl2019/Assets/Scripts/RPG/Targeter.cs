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
    public IEnumerator ExecuteRoutine(Effector effect, Result<IEnumerator> executeResult) {
        yield return InternalExecuteRoutine(effect, executeResult);
    }

    protected abstract IEnumerator InternalExecuteRoutine(Effector effect, Result<IEnumerator> result);
}
